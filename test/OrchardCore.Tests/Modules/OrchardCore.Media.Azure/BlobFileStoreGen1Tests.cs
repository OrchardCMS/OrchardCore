namespace OrchardCore.Tests.Modules.OrchardCore.Media.Azure;

/// <summary>
/// Runs all <see cref="BlobFileStoreTestsBase"/> tests with flat-namespace (Gen1) behavior.
/// </summary>
public sealed class BlobFileStoreGen1Tests : BlobFileStoreTestsBase
{
    protected override bool IsHnsEnabled => false;
}
