namespace OrchardCore.Media.Deployment;

internal static class MediaDeploymentSelection
{
    public static (string[] Files, string[] Directories) Normalize(bool includeAll, string[] files, string[] directories) =>
        includeAll ? ([], []) : (files ?? [], directories ?? []);
}
