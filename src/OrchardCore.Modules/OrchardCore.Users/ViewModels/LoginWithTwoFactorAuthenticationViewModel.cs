using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class LoginWithTwoFactorAuthenticationViewModel
{
    [Required]
    public string VerificationCode { get; set; }

    [Required]
    public string CurrentProvider { get; set; }

    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    public bool Next { get; set; }

    public bool RememberDevice { get; set; }

    [BindNever]
    public bool HasMultipleProviders { get; set; }

    [BindNever]
    public bool AllowRememberDevice { get; set; }
}
