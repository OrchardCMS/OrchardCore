using System.ComponentModel.DataAnnotations;

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

    /// <summary>
    /// For TextSecret: the secret value.
    /// </summary>
    public string SecretValue { get; set; }

    // RsaKeySecret properties
    public bool GenerateNewKey { get; set; }
    public int KeySize { get; set; } = 2048;
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }

    // X509Secret properties
    public string Thumbprint { get; set; }
    public string StoreLocation { get; set; }
    public string StoreName { get; set; }

    public IList<string> AvailableStores { get; set; } = [];

    public IList<string> AvailableTypes { get; set; } = [];
}
