using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class LoginWith2FAViewModel
{
    public bool RememberMe { get; set; }

    public string ReturnUrl { get; set; }

    [Required]
    public string TwoFactorCode { get; set; }

    public bool RememberClient { get; set; }
}
