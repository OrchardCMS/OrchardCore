namespace OrchardCore.Tests.Modules.OrchardCore.Media.Azure;

/// <summary>
/// Runs all <see cref="BlobFileStoreTestsBase"/> tests with hierarchical-namespace (Gen2 / ADLS) behavior.
/// Uses <c>UseHierarchicalNamespace = true</c> to force Gen2 code paths in Azurite.
/// </summary>
public sealed class BlobFileStoreGen2Tests : BlobFileStoreTestsBase
{
    protected override bool IsHnsEnabled => true;
}
