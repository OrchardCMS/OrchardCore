namespace OrchardCore.Indexing;

/// <summary>
/// Implemented by an <see cref="IIndexManager"/> whose provider is a server that can be unreachable, where
/// <see cref="IIndexManager.ExistsAsync(string)"/> can't tell an absent index from a server that didn't answer.
/// </summary>
public interface IRemoteIndexManager
{
    /// <summary>
    /// Checks whether an index exists.
    /// </summary>
    /// <param name="indexFullName">The full name of the index.</param>
    /// <returns>Whether the index exists, or <see langword="null"/> when the provider couldn't answer.</returns>
    Task<bool?> TryGetExistsAsync(string indexFullName);
}
