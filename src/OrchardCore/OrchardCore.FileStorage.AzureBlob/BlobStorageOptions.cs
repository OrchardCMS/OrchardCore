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
    /// Set to <c>true</c> to force HNS-aware behavior, <c>false</c> to force flat-namespace behavior,
    /// or leave <c>null</c> to auto-detect from the storage account at startup.
    /// </summary>
    public bool? UseHierarchicalNamespace { get; set; }

    /// <summary>
    /// Returns a value indicating whether the basic state of the configuration is valid.
    /// </summary>
    public virtual bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(ContainerName);
    }
}
