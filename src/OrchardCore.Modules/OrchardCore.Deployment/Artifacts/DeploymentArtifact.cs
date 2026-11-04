namespace OrchardCore.Deployment.Artifacts;

internal sealed class DeploymentArtifact
{
    public string Id { get; set; }
    public string Owner { get; set; }
    public DeploymentArtifactKind Kind { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long Length { get; set; }
    public string Sha256 { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime ExpiresUtc { get; set; }
}

internal sealed class DeploymentArtifactLease : IDisposable
{
    private readonly FileStream _guard;
    internal DeploymentArtifactLease(DeploymentArtifact artifact, FileStream stream, FileStream guard)
    {
        Artifact = artifact;
        Stream = stream;
        _guard = guard;
    }
    public DeploymentArtifact Artifact { get; }
    public Stream Stream { get; }
    public void Dispose()
    {
        try { Stream.Dispose(); }
        finally { _guard.Dispose(); }
    }
}

internal enum ArtifactDeleteResult { Missing, Deleted, Busy }

internal enum DeploymentArtifactKind { Import, Export }
