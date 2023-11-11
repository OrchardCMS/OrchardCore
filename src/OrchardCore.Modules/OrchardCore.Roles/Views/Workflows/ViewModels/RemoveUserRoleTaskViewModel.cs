using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Roles.Workflows.ViewModels
{
    public class RemoveUserRoleTaskViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string RoleName { get; set; }
    }
}
