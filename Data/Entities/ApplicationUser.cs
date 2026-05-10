using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebActionResults.Data.Entities;

public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}