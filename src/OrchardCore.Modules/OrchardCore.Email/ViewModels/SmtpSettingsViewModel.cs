using System.ComponentModel.DataAnnotations;
using OrchardCore.Email.Models;

namespace OrchardCore.Email.ViewModels
{
    public class SmtpSettingsViewModel : SmtpSettings
    {
        [Required(AllowEmptyStrings = false)]
        public string To { get; set; }
        public string Bcc { get; set; }
        public string Cc { get; set; }
        public string ReplyTo { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}