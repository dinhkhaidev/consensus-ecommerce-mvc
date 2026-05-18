using System.Security.Cryptography;
using System.Text;
using System.Web;
using Microsoft.Extensions.Configuration;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Repositories;

namespace WebActionResults.Data.Services;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(int orderId, PaymentMethod method, decimal amount);
    string GenerateVNPayUrl(Payment payment, string? returnUrl);
    string GenerateMoMoUrl(Payment payment, string? returnUrl);
    Task<Payment?> ProcessVNPayCallbackAsync(Dictionary<string, string> parameters);
    Task<Payment?> ProcessMoMoCallbackAsync(Dictionary<string, string> parameters);
    Task<Payment?> GetPaymentByOrderIdAsync(int orderId);
    Task<Payment?> GetPaymentByIdAsync(int paymentId);
    Task UpdatePaymentAsync(Payment payment);
    Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string? transactionId = null, string? errorMessage = null);
}

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IConfiguration _configuration;
    private readonly IWebSettingsService? _webSettingsService;
    private readonly IVnpayClient? _vnpayClient;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IConfiguration configuration,
        IWebSettingsService? webSettingsService = null,
        IVnpayClient? vnpayClient = null)
    {
        _paymentRepository = paymentRepository;
        _configuration = configuration;
        _webSettingsService = webSettingsService;
        _vnpayClient = vnpayClient;
    }

    private string GetVNPayUrl()
    {
        return _webSettingsService != null
            ? _webSettingsService.GetSettingAsync("VNPayUrl").GetAwaiter().GetResult()
              ?? _configuration["VNPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
            : _configuration["VNPay:BaseUrl"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    }

    private string GetMoMoUrl()
    {
        return _webSettingsService != null
            ? _webSettingsService.GetSettingAsync("MoMoUrl").GetAwaiter().GetResult()
              ?? "https://test-payment.momo.vn/v2/gateway/api/create"
            : "https://test-payment.momo.vn/v2/gateway/api/create";
    }

    private bool IsCODEnabled()
    {
        if (_webSettingsService == null) return true;
        var enabled = _webSettingsService.GetSettingAsync("EnableCOD").GetAwaiter().GetResult();
        return enabled != "false";
    }

    public async Task<Payment> CreatePaymentAsync(int orderId, PaymentMethod method, decimal amount)
    {
        if (method == PaymentMethod.COD && !IsCODEnabled())
        {
            throw new InvalidOperationException("Cash on Delivery is not currently enabled.");
        }

        var payment = new Payment
        {
            OrderId = orderId,
            Method = method,
            Amount = amount,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        return await _paymentRepository.CreateAsync(payment);
    }

    public string GenerateVNPayUrl(Payment payment, string? returnUrl)
    {
        if (_vnpayClient == null)
        {
            return GetVNPayUrl() + "?error=no_vnpay_config";
        }

        try
        {
            var request = new VnpayPaymentRequest
            {
                Money = (double)payment.Amount,
                Description = $"Thanh toan don hang #{payment.OrderId}",
                BankCode = BankCode.ANY,
                Language = DisplayLanguage.Vietnamese
            };

            var paymentUrlInfo = _vnpayClient.CreatePaymentUrl(request);
            return paymentUrlInfo.Url;
        }
        catch (Exception ex)
        {
            return GetVNPayUrl() + $"?error={Uri.EscapeDataString(ex.Message)}";
        }
    }

    public string GenerateMoMoUrl(Payment payment, string? returnUrl)
    {
        var momoConfig = _configuration.GetSection("MoMo");
        var endpoint = GetMoMoUrl();
        var partnerCode = momoConfig["PartnerCode"] ?? "";
        var accessKey = momoConfig["AccessKey"] ?? "";
        var secretKey = momoConfig["SecretKey"] ?? "";

        var orderId = payment.Id.ToString();
        var requestId = Guid.NewGuid().ToString();
        var amount = ((long)payment.Amount).ToString();
        var requestType = "captureWallet";

        var rawData = $"accessKey={accessKey}&amount={amount}&extraData=&message={payment.OrderId}&orderId={orderId}&partnerCode={partnerCode}&requestId={requestId}&requestType={requestType}";
        var signature = ComputeHMACSHA256(secretKey, rawData);

        var requestBody = new
        {
            partnerCode,
            accessKey,
            requestId,
            amount,
            orderId,
            signature,
            requestType,
            returnUrl,
            extraData = ""
        };

        var json = System.Text.Json.JsonSerializer.Serialize(requestBody);
        var base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

        return $"{endpoint}?data={base64}";
    }

    public async Task<Payment?> ProcessVNPayCallbackAsync(Dictionary<string, string> parameters)
    {
        parameters.TryGetValue("vnp_TxnRef", out var txnRef);
        if (string.IsNullOrEmpty(txnRef) || !int.TryParse(txnRef, out int paymentId))
            return null;

        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            return null;

        parameters.TryGetValue("vnp_ResponseCode", out var responseCode);
        responseCode ??= "99";

        if (responseCode == "00")
        {
            payment.Status = PaymentStatus.Paid;
            payment.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.ErrorMessage = $"VNPay error: {responseCode}";
        }

        parameters.TryGetValue("vnp_TransactionNo", out var txnNo);
        payment.TransactionId = txnNo ?? "";
        await _paymentRepository.UpdateAsync(payment);

        return payment;
    }

    public async Task<Payment?> ProcessMoMoCallbackAsync(Dictionary<string, string> parameters)
    {
        parameters.TryGetValue("orderId", out var orderId);
        if (string.IsNullOrEmpty(orderId) || !int.TryParse(orderId, out int paymentId))
            return null;

        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null)
            return null;

        parameters.TryGetValue("resultCode", out var resultCode);
        resultCode ??= "99";

        if (resultCode == "0")
        {
            payment.Status = PaymentStatus.Paid;
            payment.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            parameters.TryGetValue("message", out var message);
            payment.ErrorMessage = message ?? "MoMo payment failed";
        }

        parameters.TryGetValue("transId", out var transId);
        payment.TransactionId = transId ?? "";
        await _paymentRepository.UpdateAsync(payment);

        return payment;
    }

    public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId)
        => await _paymentRepository.GetByOrderIdAsync(orderId);

    public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        => await _paymentRepository.GetByIdAsync(paymentId);

    public async Task UpdatePaymentAsync(Payment payment)
        => await _paymentRepository.UpdateAsync(payment);

    public async Task UpdatePaymentStatusAsync(int paymentId, PaymentStatus status, string? transactionId = null, string? errorMessage = null)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return;

        payment.Status = status;
        if (transactionId != null)
            payment.TransactionId = transactionId;
        if (errorMessage != null)
            payment.ErrorMessage = errorMessage;
        if (status == PaymentStatus.Paid)
            payment.CompletedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);
    }

    private string ComputeHMACSHA256(string key, string data)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLower();
    }
}
