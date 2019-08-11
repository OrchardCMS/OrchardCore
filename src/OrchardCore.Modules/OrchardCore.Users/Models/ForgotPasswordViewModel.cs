using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string UserIdentifier { get; set; }
    }
} 