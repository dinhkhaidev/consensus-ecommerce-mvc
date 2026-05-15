using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebActionResults.Data.Entities;

public class WebSettings
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    public string? SettingValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
