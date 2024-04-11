using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Users.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Obsolete("Email property is not longer used and will be removed in future releases. Instead use Identifier.")]
        [Email.EmailAddress(ErrorMessage = "Invalid Email.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Username or Email is required.")]
        public string Identifier { get; set; }
    }
}
