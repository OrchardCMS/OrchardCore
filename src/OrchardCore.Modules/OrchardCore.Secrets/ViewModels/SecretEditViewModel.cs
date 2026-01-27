using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Secrets.ViewModels;

public class SecretEditViewModel
{
    public bool IsNew { get; set; }

    [Required]
    public string Name { get; set; }

    public string Store { get; set; }

    public string Description { get; set; }

    [Required]
    public string SecretType { get; set; }

    public string SecretTypeDisplayName { get; set; }

    public IList<string> AvailableStores { get; set; } = [];

    // TextSecret properties
    public string TextValue { get; set; }

    // RsaKeySecret properties
    public string RsaPublicKey { get; set; }
    public string RsaPrivateKey { get; set; }
    public int RsaKeySize { get; set; } = 2048;

    // X509Secret properties
    public string X509StoreLocation { get; set; }
    public string X509StoreName { get; set; }
    public string X509Thumbprint { get; set; }
}
