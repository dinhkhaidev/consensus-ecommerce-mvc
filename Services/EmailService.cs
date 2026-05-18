using System.Net;
using System.Net.Mail;
using System.Web;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace WebActionResults.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendVerificationEmailAsync(string email, string token);
    Task SendOrderConfirmationAsync(string email, string orderNumber, decimal totalAmount);

    Task SendOtpOnlyEmailAsync(string email, string token);
    Task SendLinkOnlyEmailAsync(string email, string token);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private string SmtpHost => Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST") ?? _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
    private int SmtpPort => int.TryParse(Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"), out var port) ? port : int.Parse(_configuration["Email:SmtpPort"] ?? "587");
    private string SmtpUser => Environment.GetEnvironmentVariable("EMAIL_SMTP_USER") ?? _configuration["Email:SmtpUser"] ?? "";
    private string SmtpPassword => Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD") ?? _configuration["Email:SmtpPassword"] ?? "";
    private string FromEmail => Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _configuration["Email:FromEmail"] ?? "noreply@furnish.com";
    private string FromName => Environment.GetEnvironmentVariable("EMAIL_FROM_NAME") ?? _configuration["Email:FromName"] ?? "Furnish Shop";

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(SmtpUser) || string.IsNullOrEmpty(SmtpPassword) || SmtpUser == "your-email@gmail.com")
        {
            _logger.LogWarning("Email not configured. Skipping email to {Email}. Subject: {Subject}", to, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                Credentials = new NetworkCredential(SmtpUser, SmtpPassword),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            using var message = new MailMessage
            {
                From = new MailAddress(FromEmail, FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(to));

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
        }
    }

    public async Task SendVerificationEmailAsync(string email, string token)
    {
        var subject = "Xác nhận tài khoản Furnish Shop";

        // 1. Tạo đường link chứa mã đã được mã hóa an toàn (Phương án 2)
        var encodedToken = HttpUtility.UrlEncode(token);
        var verifyUrl = $"http://localhost:5085/Account/VerifyEmailClick?email={email}&token={encodedToken}";

        // 2. Giao diện Email thiết kế chia làm 2 cách rõ ràng
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #ddd; border-radius: 10px; max-width: 500px; margin: auto; text-align: center;'>
                <h2 style='color: #333;'>Xác nhận tài khoản Furnish Shop</h2>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                
                <p style='font-size: 16px; color: #555;'><strong>CÁCH 1:</strong> Nhập mã OTP 6 số này vào trang web:</p>
                <h1 style='color: #222; letter-spacing: 10px; font-size: 36px; background: #f4f4f4; padding: 15px; border-radius: 10px; display: inline-block;'>{token}</h1>
                
                <p style='margin-top: 30px; font-size: 16px; color: #555;'><strong>CÁCH 2:</strong> Hoặc bấm thẳng vào nút bên dưới để xác thực tự động:</p>
                <a href='{verifyUrl}' style='display:inline-block; padding:15px 30px; background-color:#198754; color:#fff; text-decoration:none; border-radius:8px; font-weight:bold; font-size: 16px; margin-top: 10px;'>
                    XÁC THỰC NGAY
                </a>
            </div>";

        await SendEmailAsync(email, subject, htmlMessage);
    }

        public async Task SendOtpOnlyEmailAsync(string email, string token)
    {
        var subject = "Mã OTP xác thực tài khoản Furnish Shop";
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px; border: 1px solid #ddd; border-radius: 10px; max-width: 500px; margin: auto;'>
                <h2 style='color: #222; text-transform: uppercase;'>Mã Xác Thực OTP</h2>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='font-size: 15px; color: #555;'>Tuyệt đối không chia sẻ mã này với bất kỳ ai. Nhập mã 6 số sau đây vào trang web:</p>
                <h1 style='color: #111; letter-spacing: 10px; font-size: 38px; background: #f8f9fa; padding: 15px; border-radius: 8px; display: inline-block; border: 1px dashed #ccc;'>{token}</h1>
                <p style='color: #998; font-size: 12px; margin-top: 20px;'>Mã OTP có hiệu lực trong vòng 5 phút.</p>
            </div>";

        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendLinkOnlyEmailAsync(string email, string token)
    {
        var subject = "Liên kết kích hoạt tài khoản Furnish Shop";
        var encodedToken = HttpUtility.UrlEncode(token);
        var verifyUrl = $"http://localhost:5085/Account/VerifyEmailClick?email={email}&token={encodedToken}";
        
        var htmlMessage = $@"
            <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px; border: 1px solid #ddd; border-radius: 10px; max-width: 500px; margin: auto;'>
                <h2 style='color: #222; text-transform: uppercase;'>Kích Hoạt Tài Khoản</h2>
                <hr style='border: 0; border-top: 1px solid #eee; margin: 20px 0;' />
                <p style='font-size: 15px; color: #555;'>Vui lòng bấm vào nút liên kết dưới đây để xác thực tài khoản của bạn trên hệ thống tự động:</p>
                <div style='margin: 30px 0;'>
                    <a href='{verifyUrl}' style='display: inline-block; padding: 14px 35px; background-color: #111; color: #fff; text-decoration: none; font-weight: bold; border-radius: 4px; text-transform: uppercase; font-size: 13px; letter-spacing: 0.1rem;'>
                        Xác thực tài khoản ngay
                    </a>
                </div>
                <p style='color: #998; font-size: 12px;'>Đường dẫn sẽ hết hạn sau khi được sử dụng.</p>
            </div>";

        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendOrderConfirmationAsync(string email, string orderNumber, decimal totalAmount)
    {
        var subject = $"Order Confirmation - #{orderNumber}";
        var html = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #d47f31 0%, #e8a05f 100%); padding: 30px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>Furnish Shop</h1>
                </div>
                <div style='padding: 30px; background: #f9f9f9;'>
                    <h2 style='color: #333;'>Order Confirmed!</h2>
                    <p style='color: #666; font-size: 16px;'>Thank you for your order. Your order number is <strong>#{orderNumber}</strong>.</p>
                    <div style='background: white; padding: 20px; border-radius: 10px; margin: 20px 0;'>
                        <h3 style='color: #d47f31;'>Total: {totalAmount.ToString("N0")} VND</h3>
                    </div>
                    <p style='color: #666;'>We will notify you when your order is shipped.</p>
                </div>
            </div>";

        await SendEmailAsync(email, subject, html);
    }
}
