namespace OrchardCore.FileStorage;

/// <summary>
/// Describes the capabilities supported by a specific <see cref="IFileStore"/> implementation.
/// </summary>
public interface IFileStoreCapabilities
{
    /// <summary>
    /// Gets a value indicating whether the store has a true hierarchical namespace
    /// (i.e. directories are first-class objects, not simulated via path prefixes).
    /// </summary>
    bool HasHierarchicalNamespace { get; }

    /// <summary>
    /// Gets a value indicating whether the store supports atomic (server-side) move / rename
    /// that does not require a copy-then-delete round-trip.
    /// </summary>
    bool SupportsAtomicMove { get; }

}

/// <summary>
/// Provides a default <see cref="IFileStoreCapabilities"/> instance where all capabilities are <c>false</c>.
/// </summary>
public sealed class FileStoreCapabilities : IFileStoreCapabilities
{
    /// <summary>
    /// A shared instance that returns <c>false</c> for every capability.
    /// </summary>
    public static readonly IFileStoreCapabilities Default = new FileStoreCapabilities();

    public FileStoreCapabilities()
    {
    }

    public FileStoreCapabilities(bool hasHierarchicalNamespace, bool supportsAtomicMove)
    {
        HasHierarchicalNamespace = hasHierarchicalNamespace;
        SupportsAtomicMove = supportsAtomicMove;
    }

    public bool HasHierarchicalNamespace { get; }

    public bool SupportsAtomicMove { get; }
}
