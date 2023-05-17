using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class EnableAuthenticatorViewModel
{
    public string SharedKey { get; set; }

    public string AuthenticatorUri { get; set; }
    public string ReturnUrl { get; set; }

    [Required]
    public string Code { get; set; }
}
