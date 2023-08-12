using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class EnableSmsAuthenticatorViewModel
{
    [Required]
    public string Code { get; set; }

    public string PhoneNumber { get; set; }
}

public class RequestCodeSmsAuthenticatorViewModel
{
    public string PhoneNumber { get; set; }

    [BindNever]
    public bool AllowChangePhoneNumber { get; set; }
}
