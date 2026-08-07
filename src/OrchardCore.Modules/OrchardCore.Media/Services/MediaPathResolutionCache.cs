namespace OrchardCore.Media.Services;

/// <summary>
/// Memoizes media path resolution for the lifetime of a request.
/// </summary>
/// <remarks>
/// Resolving a path canonicalizes it against the file store, which costs up to two round-trips for a path
/// that exists and one per ancestor for one that does not — a network call each on a remote store.
/// The same path is resolved more than once per request in practice: authorizing a folder resolves it,
/// and when Secure Media is enabled the nested view check resolves it again. Listing a directory then
/// repeats that for every folder it contains.
/// <para>
/// Scoped to the request, so it needs no invalidation: a resolution cannot become stale within a single
/// request, and nothing is shared between users.
/// </para>
/// </remarks>
public sealed class MediaPathResolutionCache
{
    private readonly Dictionary<string, string> _resolved = new(StringComparer.Ordinal);

    /// <summary>
    /// Returns the cached resolution for <paramref name="path"/>, if it has already been resolved.
    /// </summary>
    public bool TryGet(string path, out string resolved)
        => _resolved.TryGetValue(path ?? string.Empty, out resolved);

    /// <summary>
    /// Records the resolution of <paramref name="path"/>.
    /// </summary>
    public void Set(string path, string resolved)
        => _resolved[path ?? string.Empty] = resolved;
}
