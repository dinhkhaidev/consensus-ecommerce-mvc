using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebActionResults.Data.Entities;

namespace WebActionResults.Models;

public partial class Account
{
    public int Id { get; set; }

    [Required(ErrorMessage = "*")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Username 3 to 20 characters")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "*")]
    [MaxLength(50, ErrorMessage = "Fullname has maximum length of 50 characters")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "*")]
    [RegularExpression(@"^[a-zA-Z0-9]{6,}$", ErrorMessage = "Minimum Length 6 letters")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Display(Name = "Email address")]
    [Required(ErrorMessage = "Email is required")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.]+\.[a-zA-Z0-9._-]+$", ErrorMessage = "Invalid Email")]
    public string Email { get; set; } = null!;

    [Display(Name = "Your phone number")]
    [Required(ErrorMessage = "*")]
    [RegularExpression(@"0[39875]\d{8}", ErrorMessage = "Mobile format invalid in Vietnam")]
    public string Phone { get; set; } = null!;

    [DataType(DataType.Date, ErrorMessage = "Invalid birthday")]
    public DateTime? Birthday { get; set; }

    public int Status { get; set; }

    public string Role { get; set; } = "Customer";

    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool? IsEmailVerified { get; set; } = false;

    public string? EmailVerificationToken { get; set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Notes { get; set; }

    // Navigation properties
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
