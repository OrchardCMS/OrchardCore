using OrchardCore.DisplayManagement;

namespace OrchardCore.DynamicCache.Models;

/// <summary>
/// Model for cache shape metadata with source-generated Arguments provider.
/// </summary>
[GenerateArgumentsProvider]
public partial class CacheShapeMetadata
{
    public string CacheId { get; set; }
    public string CacheTag { get; set; }
    public string CacheContext { get; set; }
    public string CacheDuration { get; set; }
    public bool VaryByQueryString { get; set; }
    public bool VaryByRoute { get; set; }
}
