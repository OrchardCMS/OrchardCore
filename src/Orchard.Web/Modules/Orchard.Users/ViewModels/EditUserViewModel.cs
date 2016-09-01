using System.ComponentModel.DataAnnotations;

namespace Orchard.Users.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public RoleViewModel[] Roles { get; set; }
    }
}
