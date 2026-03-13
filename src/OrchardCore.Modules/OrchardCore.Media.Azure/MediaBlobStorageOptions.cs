namespace OrchardCore.Media.Azure;

public class MediaBlobStorageOptions : MediaBlobStorageOptionsBase
{
    /// <summary>
    /// Overrides auto-detection of Hierarchical Namespace (HNS / ADLS Gen2) support.
    /// Set to <c>true</c> to force HNS-aware behavior, <c>false</c> to force flat-namespace behavior,
    /// or leave <c>null</c> to auto-detect from the storage account at startup.
    /// </summary>
    public bool? UseHierarchicalNamespace { get; set; }
}
