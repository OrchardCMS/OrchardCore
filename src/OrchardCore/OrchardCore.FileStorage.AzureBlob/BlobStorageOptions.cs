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
    /// or leave <c>null</c> (default) to auto-detect from the storage account at startup.
    /// </summary>
    /// <remarks>
    /// Auto-detection calls <c>GetAccountInfoAsync()</c>, which requires storage account-level
    /// permissions. You must set this explicitly when:
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///       <b>SAS tokens</b> — A container-scoped SAS token does not have permission to call
    ///       <c>GetAccountInfo</c>, so detection fails and falls back to flat-namespace behavior.
    ///       Set to <c>true</c> if the account is actually Gen2/HNS-enabled.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///       <b>Local emulators (Azurite)</b> — Azurite does not implement the <c>GetAccountInfo</c>
    ///       endpoint. Set to <c>true</c> together with <see cref="DfsEndpoint"/> to enable
    ///       HNS simulation in local development.
    ///     </description>
    ///   </item>
    /// </list>
    /// Without this override there is no way to use HNS-dependent features (atomic moves, real
    /// directory operations) in either of those scenarios.
    /// </remarks>
    public bool? UseHierarchicalNamespace { get; set; }

    /// <summary>
    /// Optional DFS (Data Lake Storage) endpoint URL for Gen2 operations.
    /// When set, the <see cref="Azure.Storage.Files.DataLake.DataLakeServiceClient"/> uses this
    /// endpoint instead of deriving one from the connection string.
    /// Required for local emulators (e.g. Azurite) where the DFS endpoint runs on a separate port.
    /// </summary>
    public string DfsEndpoint { get; set; }

    /// <summary>
    /// Returns a value indicating whether the basic state of the configuration is valid.
    /// </summary>
    public virtual bool IsConfigured()
    {
        return !string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(ContainerName);
    }
}
