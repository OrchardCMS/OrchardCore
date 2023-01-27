using System;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Tenants;

public static class ShellSettingsExtensions
{
    public static string[] GetFeatureProfiles(this ShellSettings shellSettings)
    {
        return shellSettings["FeatureProfile"]?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>();
    }
}
