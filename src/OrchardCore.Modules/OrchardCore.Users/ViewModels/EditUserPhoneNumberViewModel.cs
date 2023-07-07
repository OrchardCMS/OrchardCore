using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class EditUserPhoneNumberViewModel
{
    [Phone(ErrorMessage = "Invalid Phone Number.")]
    public string PhoneNumber { get; set; }

    public bool PhoneNumberConfirmed { get; set; }

    [BindNever]
    public bool AllowEditing { get; set; }
}

