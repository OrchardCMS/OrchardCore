using System.ComponentModel.DataAnnotations;

namespace Orchard.Users.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword")]
        public string PasswordConfirmation { get; set; }
    }
}
