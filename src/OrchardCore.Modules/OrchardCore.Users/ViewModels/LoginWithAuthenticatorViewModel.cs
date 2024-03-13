using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class LoginWithAuthenticatorViewModel
{
    [Required]
    public string Code { get; set; }

    [Required]
    public string Provider { get; set; }

    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    public bool RememberDevice { get; set; }

    [BindNever]
    public bool AllowRememberClient { get; set; }
}
