using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class AccountActivationViewModel
    {
        [Required]
        public string ActivationToken { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string PasswordConfirmation { get; set; }

        public string ReturnUrl { get; set; }
    }
}
