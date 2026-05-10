using System.Net;
using System.Net.Mail;
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

    private string SmtpHost => _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
    private int SmtpPort => int.Parse(_configuration["Email:SmtpPort"] ?? "587");
    private string SmtpUser => _configuration["Email:SmtpUser"] ?? "";
    private string SmtpPassword => _configuration["Email:SmtpPassword"] ?? "";
    private string FromEmail => _configuration["Email:FromEmail"] ?? "noreply@furnish.com";
    private string FromName => _configuration["Email:FromName"] ?? "Furnish Shop";

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
        var verificationUrl = $"http://localhost:5085/Account/VerifyEmail?token={token}";
        var subject = "Verify your email - Furnish Shop";
        var html = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                <div style='background: linear-gradient(135deg, #d47f31 0%, #e8a05f 100%); padding: 30px; text-align: center;'>
                    <h1 style='color: white; margin: 0;'>Furnish Shop</h1>
                </div>
                <div style='padding: 30px; background: #f9f9f9;'>
                    <h2 style='color: #333;'>Email Verification</h2>
                    <p style='color: #666; font-size: 16px;'>Thank you for registering with Furnish Shop. Please click the button below to verify your email address.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verificationUrl}' style='background: linear-gradient(135deg, #d47f31 0%, #e8a05f 100%); color: white; padding: 15px 40px; text-decoration: none; border-radius: 50px; font-weight: bold;'>Verify Email</a>
                    </div>
                    <p style='color: #999; font-size: 14px;'>Or copy this link: {verificationUrl}</p>
                    <p style='color: #999; font-size: 14px;'>This link expires in 24 hours.</p>
                </div>
            </div>";

        await SendEmailAsync(email, subject, html);
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
