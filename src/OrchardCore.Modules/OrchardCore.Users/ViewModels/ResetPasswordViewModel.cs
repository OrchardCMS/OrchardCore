using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [Email.EmailAddress(ErrorMessage = "Invalid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
        public string PasswordConfirmation { get; set; }

        public string ResetToken { get; set; }
    }
}
