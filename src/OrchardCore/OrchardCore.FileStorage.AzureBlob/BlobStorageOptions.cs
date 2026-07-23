namespace OrchardCore.FileStorage.AzureBlob;

public abstract class BlobStorageOptions
{
    /// <summary>
    /// The Azure Blob connection string.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// The Azure Blob container name.
    /// </summary>
    public string ContainerName { get; set; }

    /// <summary>
    /// The base directory path to use inside the container for this store's content.
    /// </summary>
    public string BasePath { get; set; } = "";

    /// <summary>
    /// Overrides auto-detection of Hierarchical Namespace (HNS / ADLS Gen2) support.
    /// Leave <c>null</c> (default) to auto-detect via <c>GetAccountInfo</c>.
    /// Set to <c>true</c> or <c>false</c> to select the behavior explicitly if detection fails.
    /// </summary>
    /// <remarks>
    /// Auto-detection requires storage account-level permissions. If you are using a
    /// container-scoped SAS token, detection can fail and the store will use flat-namespace
    /// operations. In that case, set this to <c>true</c> for a Gen2 (HNS-enabled) account
    /// to use native Data Lake operations, or <c>false</c> for a Gen1 account.
    /// </remarks>
    public bool? UseHierarchicalNamespace { get; set; }

    /// <summary>
    /// Returns a value indicating whether the basic state of the configuration is valid.
    /// </summary>
    public virtual bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(ContainerName);
    }
}
