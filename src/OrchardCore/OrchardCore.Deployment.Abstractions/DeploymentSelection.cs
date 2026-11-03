namespace OrchardCore.Deployment;

/// <summary>Normalizes explicit name selections shared by deployment editors and configuration contracts.</summary>
public static class DeploymentSelection
{
    /// <summary>Checks whether a required selection is present when all entries are not included.</summary>
    public static bool IsValid(bool includeAll, string[] names, bool required = true) => includeAll || !required || names is { Length: > 0 };

    /// <summary>Clears selections when all entries are included; otherwise preserves order and duplicates.</summary>
    public static string[] Normalize(bool includeAll, string[] names) => includeAll ? [] : names ?? [];

    /// <summary>Checks whether an entry passes an optional filter of exact names; no names means no filter.</summary>
    public static bool IsSelected(string[] names, string name) => names is not { Length: > 0 } || names.Contains(name, StringComparer.Ordinal);
}
