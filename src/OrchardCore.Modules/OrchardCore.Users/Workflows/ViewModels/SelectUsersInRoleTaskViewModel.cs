using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class SelectUsersInRoleTaskViewModel
    {
        [Required]
        public string PropertyName { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
