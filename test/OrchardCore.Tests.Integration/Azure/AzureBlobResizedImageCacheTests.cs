#nullable enable

using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Azure;
using OrchardCore.Media.Azure.Services;
using OrchardCore.Tests.Integration.Infrastructure;
using Xunit;

namespace OrchardCore.Tests.Integration.Azure;

/// <summary>
/// Integration tests for <see cref="AzureBlobResizedImageCache"/> that run against an Azurite
/// Azure Storage emulator started automatically by Testcontainers. The tests are skipped when
/// Docker is not available.
/// </summary>
[Collection(AzuriteCollection.Name)]
public sealed class AzureBlobResizedImageCacheTests : IAsyncLifetime
{
    private readonly AzuriteFixture _fixture;
    private string _connectionString = null!;
    private string _containerName = null!;
    private BlobContainerClient _containerClient = null!;
    private AzureBlobResizedImageCache _cache = null!;

    public AzureBlobResizedImageCacheTests(AzuriteFixture fixture) => _fixture = fixture;

    public async ValueTask InitializeAsync()
    {
        if (!DockerSupport.IsAvailable)
        {
            return;
        }

        _connectionString = _fixture.ConnectionString;
        _containerName = $"img-cache-{Guid.NewGuid():N}";

        _containerClient = new BlobServiceClient(_connectionString).GetBlobContainerClient(_containerName);
        await _containerClient.CreateIfNotExistsAsync();

        _cache = CreateCache();
    }

    public async ValueTask DisposeAsync()
    {
        if (_containerClient is not null)
        {
            try
            {
                await _containerClient.DeleteIfExistsAsync();
            }
            catch { /* best-effort cleanup */ }
        }
    }

    private AzureBlobResizedImageCache CreateCache(string basePath = "")
    {
        var options = Options.Create(new MediaBlobImageCacheOptions
        {
            ConnectionString = _connectionString,
            ContainerName = _containerName,
            BasePath = basePath,
        });
        return new AzureBlobResizedImageCache(options, NullLogger<AzureBlobResizedImageCache>.Instance);
    }

    private static MemoryStream FakeImage(string content = "image-bytes")
        => new(System.Text.Encoding.UTF8.GetBytes(content));

    private static async Task<string> ReadContentAsync(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    // ── Get miss ─────────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Get_NonExistentKey_ReturnsNull()
    {
        var result = await _cache.GetAsync("no-such-key", ".jpg");
        Assert.Null(result);
    }

    // ── Set / Get round-trip ─────────────────────────────────────────────────

    [DockerFact]
    public async Task Set_ThenGetJPEG_RoundTripsContentAndContentType()
    {
        const string body = "fake-jpeg-bytes";
        using var input = FakeImage(body);

        await _cache.SetAsync("jpeg-key", input, "image/jpeg", TimeSpan.FromDays(1));

        var result = await _cache.GetAsync("jpeg-key", ".jpg");
        Assert.NotNull(result);
        using var stream = result!.Value.Content;
        Assert.Equal("image/jpeg", result.Value.ContentType);
        Assert.Equal(body, await ReadContentAsync(stream));
    }

    [DockerFact]
    public async Task Set_ThenGetPNG_ReturnsCorrectContentType()
    {
        using var input = FakeImage();
        await _cache.SetAsync("png-key", input, "image/png", TimeSpan.FromDays(1));

        var result = await _cache.GetAsync("png-key", ".png");
        Assert.NotNull(result);
        result!.Value.Content.Dispose();
        Assert.Equal("image/png", result.Value.ContentType);
    }

    [DockerFact]
    public async Task Set_ThenGetWebP_ReturnsCorrectContentType()
    {
        using var input = FakeImage();
        await _cache.SetAsync("webp-key", input, "image/webp", TimeSpan.FromDays(1));

        var result = await _cache.GetAsync("webp-key", ".webp");
        Assert.NotNull(result);
        result!.Value.Content.Dispose();
        Assert.Equal("image/webp", result.Value.ContentType);
    }

    [DockerFact]
    public async Task Set_SameKeyOverwrites_Content()
    {
        using var v1 = FakeImage("v1");
        using var v2 = FakeImage("v2");

        await _cache.SetAsync("ow-key", v1, "image/jpeg", TimeSpan.FromDays(1));
        await _cache.SetAsync("ow-key", v2, "image/jpeg", TimeSpan.FromDays(1));

        var result = await _cache.GetAsync("ow-key", ".jpg");
        Assert.NotNull(result);
        using var stream = result!.Value.Content;
        Assert.Equal("v2", await ReadContentAsync(stream));
    }

    // ── Clear ────────────────────────────────────────────────────────────────

    [DockerFact]
    public async Task Clear_EmptyCache_Succeeds()
    {
        // ClearAsync on an empty container must not throw.
        await _cache.ClearAsync();
    }

    [DockerFact]
    public async Task Clear_Default_DeletesAllEntries()
    {
        using var a = FakeImage("a");
        using var b = FakeImage("b");
        await _cache.SetAsync("key-a", a, "image/jpeg", TimeSpan.FromDays(1));
        await _cache.SetAsync("key-b", b, "image/png", TimeSpan.FromDays(1));

        await _cache.ClearAsync();

        Assert.Null(await _cache.GetAsync("key-a", ".jpg"));
        Assert.Null(await _cache.GetAsync("key-b", ".png"));
    }

    [DockerFact]
    public async Task Clear_BasePath_OnlyDeletesEntriesUnderThatPath()
    {
        var cacheA = CreateCache(basePath: "zone-a");
        var cacheB = CreateCache(basePath: "zone-b");

        using var imgA = FakeImage("a");
        using var imgB = FakeImage("b");
        await cacheA.SetAsync("shared-key", imgA, "image/jpeg", TimeSpan.FromDays(1));
        await cacheB.SetAsync("shared-key", imgB, "image/jpeg", TimeSpan.FromDays(1));

        await cacheA.ClearAsync();

        Assert.Null(await cacheA.GetAsync("shared-key", ".jpg"));
        Assert.NotNull(await cacheB.GetAsync("shared-key", ".jpg"));
        (await cacheB.GetAsync("shared-key", ".jpg"))?.Content.Dispose();
    }

    // ── ClearStale ───────────────────────────────────────────────────────────

    [DockerFact]
    public async Task ClearStale_FreshEntries_AreRetained()
    {
        using var img = FakeImage("fresh");
        await _cache.SetAsync("fresh-key", img, "image/jpeg", TimeSpan.FromDays(1));

        // maxAge = 1 h → cutoff = UtcNow − 1 h; blobs just uploaded are newer.
        await _cache.ClearStaleAsync(TimeSpan.FromHours(1));

        var result = await _cache.GetAsync("fresh-key", ".jpg");
        Assert.NotNull(result);
        result!.Value.Content.Dispose();
    }

    [DockerFact]
    public async Task ClearStale_NegativeMaxAge_RemovesAllEntries()
    {
        using var img = FakeImage("stale");
        await _cache.SetAsync("stale-key", img, "image/jpeg", TimeSpan.FromDays(1));

        // maxAge = −1 s → cutoff = UtcNow + 1 s (slightly in the future).
        // Every blob's LastModified < UtcNow + 1 s, so all are deleted.
        await _cache.ClearStaleAsync(TimeSpan.FromSeconds(-1));

        Assert.Null(await _cache.GetAsync("stale-key", ".jpg"));
    }
}
