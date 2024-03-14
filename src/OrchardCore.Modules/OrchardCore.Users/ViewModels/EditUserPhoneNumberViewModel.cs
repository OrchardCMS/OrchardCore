using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class EditUserPhoneNumberViewModel
{
    public string PhoneNumber { get; set; }

    [BindNever]
    public bool PhoneNumberConfirmed { get; set; }

    [BindNever]
    public bool AllowEditing { get; set; }
}

