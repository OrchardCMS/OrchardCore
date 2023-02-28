using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserInformationViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [Email.EmailAddress(ErrorMessage = "Invalid Email.")]
        public string Email { get; set; }

        [BindNever]
        public bool IsEditingDisabled { get; set; }
    }
}
