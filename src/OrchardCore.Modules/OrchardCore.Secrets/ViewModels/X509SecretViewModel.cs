
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.Secrets.ViewModels;

public class X509SecretViewModel
{
    public StoreLocation? StoreLocation { get; set; }
    public StoreName? StoreName { get; set; }
    public string Thumbprint { get; set; }

    [BindNever]
    public IList<CertificateInfo> AvailableCertificates { get; } = new List<CertificateInfo>();

    [BindNever]
    public BuildEditorContext Context { get; set; }

    public class CertificateInfo
    {
        public string FriendlyName { get; set; }
        public string Issuer { get; set; }
        public DateTime NotAfter { get; set; }
        public DateTime NotBefore { get; set; }
        public StoreLocation StoreLocation { get; set; }
        public StoreName StoreName { get; set; }
        public string Subject { get; set; }
        public string ThumbPrint { get; set; }
        public bool HasPrivateKey { get; set; }
        public bool Archived { get; set; }
    }
}
