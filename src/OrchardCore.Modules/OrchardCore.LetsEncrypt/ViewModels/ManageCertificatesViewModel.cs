using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace OrchardCore.LetsEncrypt.ViewModels
{
    public class ManageCertificatesViewModel
    {
        [MinLength(1)]
        [Required]
        public string[] Hostnames { get; set; }

        [Required]
        [EmailAddress]
        public string RegistrationEmail { get; set; }

        public bool UseStaging { get; set; }

        public ISet<string> AvailableHostNames { get; set; }

        public IReadOnlyDictionary<string, HostNameSslState> HostNameSslStates { get; set; }

        public IPagedCollection<IAppServiceCertificate> InstalledCertificates { get; internal set; }
    }
}
