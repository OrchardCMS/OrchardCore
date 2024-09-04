namespace OrchardCore.ResourceManagement;

public class ResourceManagementOptions
{
    public bool UseCdn { get; set; }

    public string CdnBaseUrl { get; set; }

    public bool DebugMode { get; set; }

    public string Culture { get; set; }

    public bool AppendVersion { get; set; } = true;

    /// <summary>
    /// The prefix path that is used when a url starts with "~/".
    /// </summary>
    public string ContentBasePath { get; set; } = string.Empty;

    public HashSet<ResourceManifest> ResourceManifests { get; init; } = [];
}
