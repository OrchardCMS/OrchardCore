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
    private readonly HashSet<string> _existingDirectories = new(StringComparer.Ordinal);

    /// <summary>
    /// Declares that <paramref name="path"/> is an existing directory whose form came from the file store
    /// itself, so it resolves to itself and needs no probing.
    /// </summary>
    /// <remarks>
    /// Only call this for paths obtained by enumerating the store — never for anything derived from a
    /// request, which must be resolved so that traversal segments are collapsed.
    /// </remarks>
    public void MarkExistingDirectory(string path)
    {
        path ??= string.Empty;

        _resolved[path] = path;
        _existingDirectories.Add(path);
    }

    /// <summary>
    /// Whether <paramref name="path"/> was declared an existing directory by <see cref="MarkExistingDirectory"/>.
    /// </summary>
    public bool IsExistingDirectory(string path)
        => _existingDirectories.Contains(path ?? string.Empty);

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
