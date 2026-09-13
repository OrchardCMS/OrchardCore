namespace OrchardCore.Deployment;

/// <summary>Normalizes explicit name selections shared by deployment editors and configuration contracts.</summary>
public static class DeploymentSelection
{
    /// <summary>Clears selections when all entries are included; otherwise preserves order and duplicates.</summary>
    public static string[] Normalize(bool includeAll, string[] names) => includeAll ? [] : names ?? [];
}
