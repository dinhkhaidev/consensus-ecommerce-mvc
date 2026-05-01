using System.ComponentModel.DataAnnotations;

namespace WebActionResults.DTOs.Requests
{
    public class Register
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Username must be from 3 to 20 characters.")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(50, ErrorMessage = "Full name has maximum length of 50 characters.")]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]{6,}$", ErrorMessage = "Password must have at least 6 letters or digits.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Confirm password does not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Your phone number")]
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"0[39875]\d{8}", ErrorMessage = "Phone format is invalid for Vietnam mobile.")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid birthday.")]
        public DateTime? Birthday { get; set; }
    }
}
