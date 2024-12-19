using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class RegisterExternalLoginViewModel
{
    [BindNever]
    public bool NoUsername { get; set; }

    [BindNever]
    public bool NoEmail { get; set; }

    [BindNever]
    public bool NoPassword { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }
}
