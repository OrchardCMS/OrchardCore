using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class LoginWithRecoveryCodeViewModel
{
    [Required]
    public string RecoveryCode { get; set; }

    public string ReturnUrl { get; set; }
}
