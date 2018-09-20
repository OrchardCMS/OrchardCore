using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class RegisterUserTaskViewModel
    {
        public bool ConfirmUser { get; set; }

        [Required]
        public string ConfirmationEmailTemplate { get; set; }
    }
}
