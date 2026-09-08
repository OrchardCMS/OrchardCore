using OrchardCore.FileStorage;

namespace OrchardCore.Media.Services;

internal static class MediaFileStorePathHelper
{
    /// <summary>
    /// Resolves and sanitizes a media path, collapsing any path-traversal segments (e.g. <c>..</c>)
    /// against the actual file store so that the returned path can be safely used in authorization checks.
    /// </summary>
    public static async Task<string> ResolveAuthorizedPathAsync(this IMediaFileStore fileStore, string path)
    {
        path = fileStore.NormalizePath(Uri.UnescapeDataString(path));

        if (string.IsNullOrEmpty(path))
        {
            return string.Empty;
        }

        var file = await fileStore.GetFileInfoAsync(path);
        if (file is not null)
        {
            return fileStore.NormalizePath(file.Path);
        }

        var directory = await fileStore.GetDirectoryInfoAsync(path);
        if (directory is not null)
        {
            return fileStore.NormalizePath(directory.Path);
        }

        return await ResolveNonExistingPathAsync(fileStore, path);
    }

    private static async Task<string> ResolveNonExistingPathAsync(IMediaFileStore fileStore, string path)
    {
        var separator = fileStore.Combine("a", "b").Contains('/') ? '/' : '\\';
        var segments = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return string.Empty;
        }

        for (var i = segments.Length; i >= 0; i--)
        {
            var ancestorPath = string.Join(separator, segments[..i]);
            var ancestor = await fileStore.GetDirectoryInfoAsync(ancestorPath);
            if (ancestor is null)
            {
                continue;
            }

            return CollapseSegments(fileStore.NormalizePath(ancestor.Path), segments[i..], separator);
        }

        return CollapseSegments(string.Empty, segments, separator);
    }

    private static string CollapseSegments(string basePath, IReadOnlyList<string> extraSegments, char separator)
    {
        var resolvedSegments = new List<string>();

        if (!string.IsNullOrEmpty(basePath))
        {
            resolvedSegments.AddRange(basePath.Split(separator, StringSplitOptions.RemoveEmptyEntries));
        }

        foreach (var segment in extraSegments)
        {
            if (string.IsNullOrEmpty(segment) || segment == ".")
            {
                continue;
            }

            if (segment == "..")
            {
                if (resolvedSegments.Count > 0)
                {
                    resolvedSegments.RemoveAt(resolvedSegments.Count - 1);
                }

                continue;
            }

            resolvedSegments.Add(segment);
        }

        return string.Join(separator, resolvedSegments);
    }
}
