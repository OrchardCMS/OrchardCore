using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class EditPasswordViewModel
{
    [Required(ErrorMessage = "Username or email address is required.")]
    public string UsernameOrEmail { get; set; }

    [Required(ErrorMessage = "Current password is required.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [Required(ErrorMessage = "New password is required.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public string PasswordConfirmation { get; set; }
}
