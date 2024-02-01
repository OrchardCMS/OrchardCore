using OrchardCore.Settings;

namespace OrchardCore.ResourceManagement;

public class ResourceOptions
{
    public ResourceDebugMode ResourceDebugMode { get; set; }
    public bool UseCdn { get; set; }
    public string CdnBaseUrl { get; set; }
    public bool AppendVersion { get; set; } = true;
}
