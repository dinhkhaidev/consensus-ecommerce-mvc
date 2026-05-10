using Microsoft.AspNetCore.Identity;

namespace WebActionResults.Data.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}