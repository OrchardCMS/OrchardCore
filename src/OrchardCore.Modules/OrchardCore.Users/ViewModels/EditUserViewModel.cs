using System.ComponentModel.DataAnnotations;
using OrchardCore.Modules;

namespace OrchardCore.Users.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string PasswordConfirmation { get; set; }

        public bool DisplayPasswordFields { get; set; }

        public string TimeZone { get; set; }

        public RoleViewModel[] Roles { get; set; }

        public ITimeZone[] TimeZones { get; set; }
    }
}
