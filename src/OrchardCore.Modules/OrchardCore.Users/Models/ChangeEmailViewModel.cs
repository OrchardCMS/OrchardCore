using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class ChangeEmailViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
} 