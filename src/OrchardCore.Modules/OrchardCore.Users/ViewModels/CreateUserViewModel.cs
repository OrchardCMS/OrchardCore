
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool IsEnabled { get; set; }

        public RoleViewModel[] Roles { get; set; }

        public bool SendActivationEmail { get; set; }
    }
}
