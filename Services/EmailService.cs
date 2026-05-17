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
        var subject = "Xận nhận tài khoản Furnish Shop";
        
        // BẮT BUỘC: Mã hóa Token để biến các ký tự +, /, = thành mã an toàn trên URL
        var encodedToken = HttpUtility.UrlEncode(token);
        
        // Đảm bảo dùng đúng http (không có s) và đúng cổng 5085 của bạn
        var verifyUrl = $"http://localhost:5085/Account/VerifyEmailClick?email={email}&token={encodedToken}";
        
        var htmlMessage = $@"
            <h2>Xin chào!</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản. Vui lòng bấm vào nút bên dưới để xác thực:</p>
            <a href='{verifyUrl}' style='display:inline-block; padding:12px 24px; background-color:#222; color:#fff; text-decoration:none; border-radius:6px; font-weight:bold; text-uppercase:true; font-size:14px;'>
                Xác thực tài khoản ngay
            </a>";

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
