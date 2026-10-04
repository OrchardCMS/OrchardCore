namespace OrchardCore.Secrets.AzureKeyVault;

/// <summary>
/// Configuration options for Azure Key Vault secret store.
/// </summary>
public class AzureKeyVaultSecretStoreOptions
{
    /// <summary>
    /// Gets or sets the service key of the host-registered <c>SecretClient</c> to share.
    /// This must not be the feature's own service key, <c>OrchardCore.Secrets.AzureKeyVault</c>.
    /// </summary>
    public string AzureClient { get; set; }

    /// <summary>
    /// Gets or sets the application namespace prefix for Key Vault secret names. Defaults to "oc".
    /// </summary>
    public string NamePrefix { get; set; } = "oc";
}
