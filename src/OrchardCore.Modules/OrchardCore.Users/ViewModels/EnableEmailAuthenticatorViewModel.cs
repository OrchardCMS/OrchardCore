using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels;

public class EnableEmailAuthenticatorViewModel
{
    [Required]
    public string Code { get; set; }
}
