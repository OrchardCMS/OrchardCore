namespace OrchardCore.ContentTypes.Deployment;

// Shared by existing admin editors and explicit deployment configuration contracts.
internal static class ContentDefinitionSelection
{
    private static readonly char[] s_separators = [' ', ','];

    public static (string[] Types, string[] Parts) Normalize(bool includeAll, string[] types, string[] parts) =>
        includeAll ? ([], []) : (types ?? [], parts?.Distinct().ToArray() ?? []);

    // Deletion can reference definitions present only on the destination tenant.
    public static string[] ParseDeletionNames(string names) => names?.Split(s_separators, StringSplitOptions.RemoveEmptyEntries) ?? [];
}
