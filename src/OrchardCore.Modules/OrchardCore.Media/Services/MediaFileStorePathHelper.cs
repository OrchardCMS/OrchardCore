using OrchardCore.FileStorage;

namespace OrchardCore.Media.Services;

internal static class MediaFileStorePathHelper
{
    /// <summary>
    /// Resolves a media path, reusing the result if the same path was already resolved during this request.
    /// </summary>
    public static async Task<string> ResolveAuthorizedPathAsync(
        this IMediaFileStore fileStore,
        string path,
        MediaPathResolutionCache cache)
    {
        if (cache is null)
        {
            return await fileStore.ResolveAuthorizedPathAsync(path);
        }

        if (cache.TryGet(path, out var cached))
        {
            return cached;
        }

        var resolved = await fileStore.ResolveAuthorizedPathAsync(path);

        cache.Set(path, resolved);

        // The resolved form resolves to itself, so authorizing it again — as the nested view check does —
        // costs nothing.
        cache.Set(resolved, resolved);

        return resolved;
    }

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

    /// <summary>
    /// How many ancestors are probed while anchoring a path that does not exist.
    /// </summary>
    /// <remarks>
    /// The path is supplied by the caller, so without a bound a request naming a deeply nested
    /// non-existent path would cost one file-store round-trip per segment — a network call each on a
    /// remote store, and an inexpensive request to forge. Media hierarchies are far shallower than this
    /// in practice; beyond it the path is collapsed lexically instead, which anchors at the root and is
    /// the more conservative outcome.
    /// </remarks>
    private const int MaxProbedAncestors = 16;

    private static async Task<string> ResolveNonExistingPathAsync(IMediaFileStore fileStore, string path)
    {
        var separator = fileStore.Combine("a", "b").Contains('/') ? '/' : '\\';
        var segments = path.Split(separator, StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length == 0)
        {
            return string.Empty;
        }

        var deepest = segments.Length;
        var shallowest = Math.Max(0, deepest - MaxProbedAncestors);

        for (var i = deepest; i >= shallowest; i--)
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
