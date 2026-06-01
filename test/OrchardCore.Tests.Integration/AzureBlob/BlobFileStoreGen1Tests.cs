using Xunit;

namespace OrchardCore.Tests.Integration.AzureBlob;

/// <summary>
/// Runs all <see cref="BlobFileStoreTestsBase"/> tests with flat-namespace (Gen1) behavior.
/// </summary>
public sealed class BlobFileStoreGen1Tests : BlobFileStoreTestsBase
{
    protected override bool? UseHierarchicalNamespaceOverride => false;

    [AzuriteFact]
    public async Task CreateDirectory_UsesMarkerFile()
    {
        await TryCreateDirectoryAsync("gen1-dir");

        // Gen1 simulates directories with a marker blob — verify it exists.
        var blobs = new List<string>();
        await foreach (var blob in ContainerClient.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, "gen1-dir/", CancellationToken.None))
        {
            blobs.Add(blob.Name);
        }

        Assert.Single(blobs);
        Assert.EndsWith("OrchardCore.Media.txt", blobs[0]);
    }
}
