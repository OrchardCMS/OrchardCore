using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Email.ViewModels
{
    public class SmtpSettingsViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string To { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email.")]
        public string Sender { get; set; }

        public string Bcc { get; set; }

        public string Cc { get; set; }

        public string ReplyTo { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
