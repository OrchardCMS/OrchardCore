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
    /// Gets or sets the Tenant ID for Azure AD authentication.
    /// If not provided, DefaultAzureCredential will be used.
    /// </summary>
    public string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the Client ID for Azure AD authentication.
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the Client Secret for Azure AD authentication.
    /// </summary>
    public string ClientSecret { get; set; }
}
