using System.ComponentModel.DataAnnotations;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.ViewModels;

public class ProfileViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? Birthday { get; set; }

    public string? AvatarUrl { get; set; }

    public List<AddressViewModel> Addresses { get; set; } = new();

    public static ProfileViewModel FromEntity(Account account) => new()
    {
        Id = account.Id,
        UserName = account.UserName ?? string.Empty,
        Email = account.Email ?? string.Empty,
        FullName = account.FullName ?? string.Empty,
        PhoneNumber = account.Phone,
        Birthday = account.Birthday,
        AvatarUrl = account.AvatarUrl
    };
}

public class AddressViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string AddressLine { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string? Ward { get; set; }

    [Required]
    [StringLength(100)]
    public string? District { get; set; }

    [Required]
    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public static AddressViewModel FromEntity(Address address) => new()
    {
        Id = address.Id,
        FullName = address.FullName,
        Phone = address.Phone,
        AddressLine = address.AddressLine,
        Ward = address.Ward,
        District = address.District,
        City = address.City,
        PostalCode = address.PostalCode,
        IsDefault = address.IsDefault
    };
}

public class AddressEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string AddressLine { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ward is required")]
    [StringLength(100)]
    public string? Ward { get; set; }

    [Required(ErrorMessage = "District is required")]
    [StringLength(100)]
    public string? District { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public static AddressEditViewModel FromEntity(Address address) => new()
    {
        Id = address.Id,
        FullName = address.FullName,
        Phone = address.Phone,
        AddressLine = address.AddressLine,
        Ward = address.Ward,
        District = address.District,
        City = address.City,
        PostalCode = address.PostalCode,
        IsDefault = address.IsDefault
    };
}
