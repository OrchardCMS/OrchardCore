using Azure.Storage.Files.DataLake;
using OrchardCore.FileStorage;
using Xunit;

namespace OrchardCore.Tests.Integration.AzureBlob;

/// <summary>
/// Runs all <see cref="BlobFileStoreTestsBase"/> tests with Gen2 (HNS) behavior.
/// Auto-detection via <c>GetAccountInfo</c> is used — no <c>UseHierarchicalNamespace</c> override.
/// </summary>
public sealed class BlobFileStoreGen2Tests : BlobFileStoreTestsBase
{
    protected override async Task CreateContainerAsync(string connectionString, string containerName)
        => await new DataLakeServiceClient(connectionString)
            .GetFileSystemClient(containerName)
            .CreateIfNotExistsAsync();

    [AzuriteFact]
    public async Task CreateDirectory_UsesNoMarkerFile()
    {
        await TryCreateDirectoryAsync("gen2-dir");

        // Gen2 creates real directory objects via the DataLake API — no marker blob.
        var blobs = new List<string>();
        await foreach (var blob in ContainerClient.GetBlobsAsync(Azure.Storage.Blobs.Models.BlobTraits.None, Azure.Storage.Blobs.Models.BlobStates.None, "gen2-dir/", CancellationToken.None))
        {
            blobs.Add(blob.Name);
        }

        Assert.Empty(blobs);
    }

    [AzuriteFact]
    public async Task CreateDirectory_Nested_CreatesIntermediateDirectories()
    {
        await TryCreateDirectoryAsync("a/b/c");

        Assert.NotNull(await GetDirectoryInfoAsync("a/b/c"));
        Assert.NotNull(await GetDirectoryInfoAsync("a/b"));
        Assert.NotNull(await GetDirectoryInfoAsync("a"));
    }

    [AzuriteFact]
    public async Task GetDirectoryContent_Flat_IncludesGen2Directories()
    {
        await TryCreateDirectoryAsync("flat-gen2");
        await CreateTestFileAsync("flat-gen2/file.txt");
        await TryCreateDirectoryAsync("flat-gen2/subdir");
        await CreateTestFileAsync("flat-gen2/subdir/nested.txt");

        var entries = new List<IFileStoreEntry>();
        await foreach (var entry in GetDirectoryContentAsync("flat-gen2", includeSubDirectories: true))
        {
            entries.Add(entry);
        }

        // Gen2 flat listing should detect directories via hdi_isfolder metadata.
        Assert.Contains(entries, e => e.IsDirectory && e.Name == "subdir");
        Assert.Contains(entries, e => !e.IsDirectory && e.Name == "file.txt");
        Assert.Contains(entries, e => !e.IsDirectory && e.Name == "nested.txt");
    }

    [AzuriteFact]
    public async Task MoveFile_IsAtomic()
    {
        // Gen2 move uses DataLake RenameAsync which is an atomic server-side operation.
        await CreateTestFileAsync("atomic-src.txt", "atomic");

        await MoveFileAsync("atomic-src.txt", "atomic-dst.txt");

        // Source should not exist and destination should have the content.
        Assert.Null(await GetFileInfoAsync("atomic-src.txt"));
        Assert.Equal("atomic", await ReadFileContentAsync("atomic-dst.txt"));
    }

    [AzuriteFact]
    public async Task GetDirectoryInfo_AfterDeletingDirectory_ReturnsNull()
    {
        await TryCreateDirectoryAsync("temp-dir");
        Assert.NotNull(await GetDirectoryInfoAsync("temp-dir"));

        await TryDeleteDirectoryAsync("temp-dir");

        Assert.Null(await GetDirectoryInfoAsync("temp-dir"));
    }

    [AzuriteFact]
    public async Task CreateDirectory_EmptyDirectory_ExistsWithNoContent()
    {
        await TryCreateDirectoryAsync("empty-gen2-dir");

        var info = await GetDirectoryInfoAsync("empty-gen2-dir");
        Assert.NotNull(info);
        Assert.True(info.IsDirectory);

        var entries = new List<IFileStoreEntry>();
        await foreach (var entry in GetDirectoryContentAsync("empty-gen2-dir"))
        {
            entries.Add(entry);
        }

        // A real Gen2 directory should exist even with no files inside.
        Assert.Empty(entries);
    }
}
