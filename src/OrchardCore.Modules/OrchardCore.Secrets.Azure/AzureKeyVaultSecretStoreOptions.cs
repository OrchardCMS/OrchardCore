namespace OrchardCore.Secrets.Azure;

/// <summary>
/// Configuration options for Azure Key Vault secret store.
/// </summary>
public class AzureKeyVaultSecretStoreOptions
{
    /// <summary>
    /// Gets or sets the Azure Key Vault URI (e.g., https://myvault.vault.azure.net/).
    /// </summary>
    public string VaultUri { get; set; }

    /// <summary>
    /// Gets or sets the application namespace prefix for Key Vault secret names. Defaults to "oc".
    /// </summary>
    public string NamePrefix { get; set; } = "oc";

    /// <summary>
    /// Gets or sets the credential provider. An explicit selection is required.
    /// </summary>
    public AzureKeyVaultCredentialType? CredentialType { get; set; }

    /// <summary>
    /// Gets or sets the Microsoft Entra tenant ID for client secret or workload identity authentication.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the client ID for client secret, workload identity, or user-assigned managed identity authentication.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the Client Secret for Azure AD authentication.
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the federated token file path for workload identity authentication.
    /// </summary>
    public string TokenFilePath { get; set; }
}
