using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class LoginWithAuthenticatorViewModel
{
    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    [Required]
    public string Provider { get; set; }

    [Required]
    public string TwoFactorCode { get; set; }

    public bool RememberDevice { get; set; }

    [BindNever]
    public bool AllowRememberClient { get; set; }
}
