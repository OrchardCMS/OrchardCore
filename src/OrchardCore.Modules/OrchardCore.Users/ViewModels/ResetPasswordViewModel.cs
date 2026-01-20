using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class ResetPasswordViewModel
{
    [Obsolete("Email property is no longer used and will be removed in future releases. Instead use UsernameOrEmail.")]
    [Email.EmailAddress(ErrorMessage = "Invalid Email.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Username or email address is required.")]
    public string UsernameOrEmail { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public string PasswordConfirmation { get; set; }

    public string ResetToken { get; set; }
}
