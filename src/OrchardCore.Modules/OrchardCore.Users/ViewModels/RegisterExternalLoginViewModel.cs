using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class RegisterExternalLoginViewModel
    {
        public bool NoUsername { get; set; }

        public bool NoEmail { get; set; }

        public bool NoPassword { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [Email.EmailAddress(ErrorMessage = "Invalid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Passowrd is required.")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "Password must be between 6 and 100 characters.", MinimumLength = 6)]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Confirm Password do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
