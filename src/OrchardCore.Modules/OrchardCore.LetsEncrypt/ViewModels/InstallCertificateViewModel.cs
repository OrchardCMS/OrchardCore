using System.ComponentModel.DataAnnotations;

namespace OrchardCore.LetsEncrypt.ViewModels
{
    public class InstallCertificateViewModel
    {
        [MinLength(1)]
        [Required]
        public string[] Hostnames { get; set; }

        [Required]
        [EmailAddress]
        public string RegistrationEmail { get; set; }

        public bool UseStaging { get; set; }
    }
}
