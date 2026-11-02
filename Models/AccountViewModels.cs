using System.ComponentModel.DataAnnotations;

namespace FoundationManagement.Models
{
    // Wrapper so one page can hold both sign-up forms
    public class RegisterPageViewModel
    {
        public string ActiveTab { get; set; } = "donor";   // "donor" or "foundation"
        public RegisterDonorViewModel Donor { get; set; } = new();
        public RegisterFoundationViewModel Foundation { get; set; } = new();
    }

    public class RegisterDonorViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Full name")]
        public string FullName { get; set; } = "";

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = "";

        [Phone]
        public string? Phone { get; set; }

        [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class RegisterFoundationViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Foundation name")]
        public string FoundationName { get; set; } = "";

        // e.g. "mealcare.org" - staff usernames will be name@mealcare.org
        [Required, StringLength(100)]
        [RegularExpression(@"^(?!-)[a-z0-9-]+(\.[a-z0-9-]+)+$", ErrorMessage = "Enter a domain like mealcare.org (lowercase, no spaces).")]
        [Display(Name = "Foundation email domain")]
        public string EmailDomain { get; set; } = "";

        [StringLength(50)]
        [Display(Name = "Registration number")]
        public string? RegistrationNumber { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Your full name")]
        public string ManagerName { get; set; } = "";

        [Required, EmailAddress, StringLength(150)]
        [Display(Name = "Your email")]
        public string ManagerEmail { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class AddStaffViewModel
    {
        [Required, StringLength(100)]
        [Display(Name = "Staff member's full name")]
        public string FullName { get; set; } = "";

        // Only the part before the @ - the domain is added server-side
        [Required, StringLength(40)]
        [RegularExpression(@"^[a-z0-9._-]+$", ErrorMessage = "Use lowercase letters, numbers, dots, dashes or underscores.")]
        [Display(Name = "Username")]
        public string UsernameLocalPart { get; set; } = "";
    }

    public class StaffRowViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Username { get; set; } = "";
        public bool MustChangePassword { get; set; }
    }

    public class StaffPageViewModel
    {
        public string FoundationName { get; set; } = "";
        public string EmailDomain { get; set; } = "";
        public AddStaffViewModel NewStaff { get; set; } = new();
        public List<StaffRowViewModel> Staff { get; set; } = new();

        // Shown once, right after an account is created or a password reset
        public string? NewUsername { get; set; }
        public string? TempPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; } = "";

        [Required, StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = "";

        [Required, Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; } = "";
    }
}