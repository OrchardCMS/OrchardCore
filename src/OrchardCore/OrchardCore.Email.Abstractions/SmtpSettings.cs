using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Email
{
    public class SmtpSettings
    {
        [Required(AllowEmptyStrings = false), EmailAddress]

        public string DefaultSender { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Host { get; set; }
        [Range(0, 65535)]
        public int Port { get; set; } = 25;
        public bool EnableSsl { get; set; }
        public bool RequireCredentials { get; set; }
        public bool UseDefaultCredentials { get; set; }

        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
