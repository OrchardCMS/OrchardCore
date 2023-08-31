using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels;

public class EditUserEmailViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    [Email.EmailAddress(ErrorMessage = "Invalid Email.")]
    public string Email { get; set; }

    [BindNever]
    public bool AllowEditing { get; set; }
}

