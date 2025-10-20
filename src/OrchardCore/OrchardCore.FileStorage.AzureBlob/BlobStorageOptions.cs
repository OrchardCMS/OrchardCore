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
    /// The name of the Azure credential configuration to use for authentication.
    /// This should match a credential entry defined in your application's Azure settings.
    /// </summary>
    public string CredentialName { get; set; }

    /// <summary>
    /// The URI of the Azure Storage account (e.g., "https://youraccount.blob.core.windows.net").
    /// This should point to the root of the storage account, not to a specific container.
    /// If not provided, a connection string must be supplied, from which this URI will be derived automatically.
    /// </summary>
    public Uri StorageAccountUri { get; set; }

    /// <summary>
    /// Returns a value indicating whether the basic state of the configuration is valid.
    /// </summary>
    public virtual bool IsConfigured()
    {
        if (string.IsNullOrEmpty(ContainerName))
        {
            return false;
        }

        if (StorageAccountUri is not null)
        {
            return true;
        }

        return !string.IsNullOrEmpty(ConnectionString);
    }
}
