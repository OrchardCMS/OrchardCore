using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.Workflows.ViewModels
{
    public class SelectUsersInRoleTaskViewModel
    {
        [Required]
        public string OutputKeyName { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
