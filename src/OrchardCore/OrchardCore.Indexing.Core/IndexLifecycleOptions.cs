namespace OrchardCore.Indexing.Core;

/// <summary>Declares providers verified for remote content-index lifecycle operations.</summary>
public sealed class IndexLifecycleOptions
{
    /// <summary>Gets provider names enabled for the remote lifecycle contract.</summary>
    public HashSet<string> RemoteProviders { get; } = new(StringComparer.OrdinalIgnoreCase);
}
