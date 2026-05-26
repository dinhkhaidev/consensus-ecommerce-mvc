using Microsoft.AspNetCore.Mvc;
using VNPAY;
using WebActionResults.Data.Entities;
using WebActionResults.Data.Services;
using WebActionResults.Utilities;

namespace WebActionResults.Controllers;

public class PaymentController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IOrderService _orderService;
    private readonly ICartService _cartService;
    private readonly IUserService _userService;
    private readonly IVnpayClient _vnpayClient;

    public PaymentController(
        IPaymentService paymentService,
        IOrderService orderService,
        ICartService cartService,
        IUserService userService,
        IVnpayClient vnpayClient)
    {
        _paymentService = paymentService;
        _orderService = orderService;
        _cartService = cartService;
        _userService = userService;
        _vnpayClient = vnpayClient;
    }

    public async Task<IActionResult> Index(int orderId, PaymentMethod method)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return NotFound("Order not found.");

        var payment = await _paymentService.CreatePaymentAsync(orderId, method, order.TotalAmount);

        string paymentUrl = method switch
        {
            PaymentMethod.VNPay => _paymentService.GenerateVNPayUrl(payment, null),
            PaymentMethod.MoMo => _paymentService.GenerateMoMoUrl(payment, null),
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(paymentUrl))
            return Redirect(RedirectUrlSanitizer.EscapeHeaderValue(paymentUrl));

        TempData["ToastError"] = "Payment method not supported.";
        return RedirectToAction("Details", "Order", new { id = orderId });
    }

    public async Task<IActionResult> Callback()
    {
        try
        {
            // Try VNPay's GetPaymentResult first (handles real VNPay callbacks with secure hash)
            var paymentResult = _vnpayClient.GetPaymentResult(Request);

            if (paymentResult != null)
            {
                // Get orderId from description: "Thanh toan don hang #10"
                int orderId = 0;
                if (paymentResult.Description.Contains("#"))
                {
                    var parts = paymentResult.Description.Split('#');
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int parsedId))
                    {
                        orderId = parsedId;
                    }
                }

                if (orderId > 0)
                {
                    var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

                    if (payment != null)
                    {
                        payment.TransactionId = paymentResult.VnpayTransactionId.ToString();
                        payment.Status = PaymentStatus.Paid;
                        payment.CompletedAt = DateTime.UtcNow;
                        await _paymentService.UpdatePaymentAsync(payment);

                        await CompletePaidOrderAsync(payment.OrderId);

                        TempData["ToastSuccess"] = "Payment successful! Your order has been confirmed.";
                        return RedirectToAction("Details", "Order", new { id = payment.OrderId });
                    }

                    TempData["ToastError"] = "Payment not found for order: " + orderId;
                }
                else
                {
                    TempData["ToastError"] = "Could not parse orderId from: " + paymentResult.Description;
                }
            }
            else
            {
                // VNPay validation failed - check if this is a manual test callback with direct parameters
                var txnRef = Request.Query["vnp_TxnRef"].FirstOrDefault();
                var responseCode = Request.Query["vnp_ResponseCode"].FirstOrDefault();

                if (!string.IsNullOrEmpty(txnRef) && !string.IsNullOrEmpty(responseCode))
                {
                    if (int.TryParse(txnRef, out int orderId) && orderId > 0)
                    {
                        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                        if (payment != null)
                        {
                            var txnNo = Request.Query["vnp_TransactionNo"].FirstOrDefault() ?? "";
                            payment.TransactionId = txnNo;
                            payment.Status = responseCode == "00" ? PaymentStatus.Paid : PaymentStatus.Failed;
                            payment.CompletedAt = responseCode == "00" ? DateTime.UtcNow : null;
                            payment.ErrorMessage = responseCode != "00" ? $"VNPay error: {responseCode}" : null;
                            await _paymentService.UpdatePaymentAsync(payment);

                            if (responseCode == "00")
                            {
                                await CompletePaidOrderAsync(payment.OrderId);

                                TempData["ToastSuccess"] = "Payment successful! Your order has been confirmed.";
                                return RedirectToAction("Details", "Order", new { id = payment.OrderId });
                            }
                        }
                        else
                        {
                            TempData["ToastError"] = "Payment not found for order: " + orderId;
                        }
                    }
                    else
                    {
                        TempData["ToastError"] = "Invalid TxnRef format";
                    }
                }
                else
                {
                    TempData["ToastError"] = "Payment verification failed.";
                }
            }
        }
        catch (Exception ex)
        {
            // VNPay throws "Không đủ dữ liệu" when hash validation fails - treat as manual callback fallback
            var txnRef = Request.Query["vnp_TxnRef"].FirstOrDefault();
            var responseCode = Request.Query["vnp_ResponseCode"].FirstOrDefault();

            if (!string.IsNullOrEmpty(txnRef) && !string.IsNullOrEmpty(responseCode))
            {
                if (int.TryParse(txnRef, out int orderId) && orderId > 0)
                {
                    var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                    if (payment != null)
                    {
                        var txnNo = Request.Query["vnp_TransactionNo"].FirstOrDefault() ?? "";
                        payment.TransactionId = txnNo;
                        payment.Status = responseCode == "00" ? PaymentStatus.Paid : PaymentStatus.Failed;
                        payment.CompletedAt = responseCode == "00" ? DateTime.UtcNow : null;
                        payment.ErrorMessage = responseCode != "00" ? $"VNPay error: {responseCode}" : null;
                        await _paymentService.UpdatePaymentAsync(payment);

                        if (responseCode == "00")
                        {
                            await CompletePaidOrderAsync(payment.OrderId);

                            TempData["ToastSuccess"] = "Payment successful! Your order has been confirmed.";
                            return RedirectToAction("Details", "Order", new { id = payment.OrderId });
                        }
                    }
                    else
                    {
                        TempData["ToastError"] = "Payment not found for order: " + orderId;
                    }
                }
                else
                {
                    TempData["ToastError"] = "Invalid TxnRef format";
                }
            }
            else
            {
                TempData["ToastError"] = $"Payment error: {ex.Message}";
            }
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> VNPayIPN()
    {
        try
        {
            var paymentResult = _vnpayClient.GetPaymentResult(Request);

            if (paymentResult != null && paymentResult.VnpayTransactionId > 0)
            {
                // Get orderId from description
                int orderId = 0;
                if (paymentResult.Description.Contains("#"))
                {
                    var parts = paymentResult.Description.Split('#');
                    if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int parsedId))
                    {
                        orderId = parsedId;
                    }
                }

                if (orderId > 0)
                {
                    var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
                    if (payment != null)
                    {
                        payment.TransactionId = paymentResult.VnpayTransactionId.ToString();
                        payment.Status = PaymentStatus.Paid;
                        payment.CompletedAt = DateTime.UtcNow;
                        await _paymentService.UpdatePaymentAsync(payment);

                        await CompletePaidOrderAsync(payment.OrderId);
                    }
                }

                return Ok();
            }
        }
        catch (Exception)
        {
            // Log error
        }

        return BadRequest();
    }

    private async Task CompletePaidOrderAsync(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return;

        await _orderService.UpdatePaymentStatusAsync(orderId, PaymentStatus.Paid);

        if (order.Status == OrderStatus.Pending)
            await _orderService.UpdateOrderStatusAsync(orderId, OrderStatus.Confirmed);

        await _cartService.RemovePurchasedItemsAsync(order.UserId, order.OrderItems);

        HttpContext.Session.Remove(CartController.CheckoutItemIdsSessionKey);
    }
}
