using System;
using OrchardCore.Settings;

namespace OrchardCore.ResourceManagement.Core;

public class ResourceSetting
{
    public ResourceDebugMode ResourceDebugMode { get; set; }
    public bool UseCdn { get; set; }
    public string CdnBaseUrl { get; set; }
    public bool AppendVersion { get; set; } = true;
    public string ContentBasePath { get; set; } = String.Empty;
}
