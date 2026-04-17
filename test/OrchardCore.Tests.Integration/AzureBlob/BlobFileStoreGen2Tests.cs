using OrchardCore.FileStorage;
using Xunit;

namespace OrchardCore.Tests.Integration.AzureBlob;

/// <summary>
/// Runs all <see cref="BlobFileStoreTestsBase"/> tests with hierarchical-namespace (Gen2 / ADLS) behavior.
/// Uses <c>UseHierarchicalNamespace = true</c> to force Gen2 code paths in Azurite.
/// </summary>
public sealed class BlobFileStoreGen2Tests : BlobFileStoreTestsBase
{
    protected override bool IsHnsEnabled => true;

    [Fact]
    public async Task CreateDirectory_Nested_CreatesIntermediateDirectories()
    {
        await TryCreateDirectoryAsync("a/b/c");

        Assert.NotNull(await GetDirectoryInfoAsync("a/b/c"));
        Assert.NotNull(await GetDirectoryInfoAsync("a/b"));
        Assert.NotNull(await GetDirectoryInfoAsync("a"));
    }

    [Fact]
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

    [Fact]
    public async Task MoveFile_IsAtomic()
    {
        // Gen2 move uses DataLake RenameAsync which is an atomic server-side operation.
        Assert.True(Capabilities.SupportsAtomicMove);

        await CreateTestFileAsync("atomic-src.txt", "atomic");

        await MoveFileAsync("atomic-src.txt", "atomic-dst.txt");

        // Source should not exist and destination should have the content.
        Assert.Null(await GetFileInfoAsync("atomic-src.txt"));
        Assert.Equal("atomic", await ReadFileContentAsync("atomic-dst.txt"));
    }

    [Fact]
    public async Task GetDirectoryInfo_AfterDeletingDirectory_ReturnsNull()
    {
        await TryCreateDirectoryAsync("temp-dir");
        Assert.NotNull(await GetDirectoryInfoAsync("temp-dir"));

        await TryDeleteDirectoryAsync("temp-dir");

        Assert.Null(await GetDirectoryInfoAsync("temp-dir"));
    }

    [Fact]
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
