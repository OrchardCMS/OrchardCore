namespace OrchardCore.Secrets.Azure.Models;

public class SecretsKeyVaultOptions
{
    public string KeyVaultName { get; set; }
    public string Prefix { get; set; }
}
