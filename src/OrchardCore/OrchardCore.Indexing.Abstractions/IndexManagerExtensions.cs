namespace OrchardCore.Indexing;

/// <summary>
/// Extensions of <see cref="IIndexManager"/>.
/// </summary>
public static class IndexManagerExtensions
{
    /// <summary>
    /// Checks whether an index exists, telling an absent index from a provider that couldn't answer when it can.
    /// </summary>
    /// <param name="indexManager">The manager of the index provider.</param>
    /// <param name="indexFullName">The full name of the index.</param>
    /// <returns>Whether the index exists, or <see langword="null"/> when the provider couldn't answer.</returns>
    public static async Task<bool?> TryGetExistsAsync(this IIndexManager indexManager, string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(indexManager);

        return indexManager is IRemoteIndexManager remote
            ? await remote.TryGetExistsAsync(indexFullName)
            : await indexManager.ExistsAsync(indexFullName);
    }
}
