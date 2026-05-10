using System.ComponentModel.DataAnnotations;

namespace WebActionResults.ViewModels;

public class VerifyEmailViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;
}