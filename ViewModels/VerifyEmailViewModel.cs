using System.ComponentModel.DataAnnotations;

namespace WebActionResults.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } // Dòng bạn đang bị thiếu

        [Required]
        public string Token { get; set; } // Thường dùng để chứa mã OTP/Token gửi qua email
    }
}