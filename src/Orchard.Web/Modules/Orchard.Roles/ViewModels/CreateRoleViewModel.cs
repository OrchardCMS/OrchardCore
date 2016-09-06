using System.ComponentModel.DataAnnotations;

namespace Orchard.Roles.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required]
        public string RoleName { get; set; }
    }
}
