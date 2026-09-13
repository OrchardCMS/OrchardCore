using System.Net;

namespace OrchardCore.Cli.Tests;

public class CacheServiceTests
{
    [Fact]
    public async Task GetOrRefreshAsync_WhenServerReturnsNotModified_ReusesCacheAndExtendsTtl()
    {
        var tenantUrl = "https://example.com/tenant/";
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(GetOrRefreshAsync_WhenServerReturnsNotModified_ReusesCacheAndExtendsTtl)));
        var service = new CacheService(paths);
        await service.WriteAsync(tenantUrl, CacheKind.Manifest, new CachedContentRecord
        {
            Url = tenantUrl,
            ETag = "\"abc\"",
            Content = "{\"ok\":true}",
            FetchedAt = DateTimeOffset.UtcNow.AddHours(-2),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
        }, CancellationToken.None);

        var result = await service.GetOrRefreshAsync(
            tenantUrl,
            CacheKind.Manifest,
            new Uri(tenantUrl),
            TimeSpan.FromMinutes(10),
            force: false,
            (etag, _) => Task.FromResult(CreateResponse(HttpStatusCode.NotModified, etag)),
            CancellationToken.None);

        Assert.True(result.FromCache);
        Assert.False(result.IsStale);
        Assert.True(result.CacheRecord.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task GetOrRefreshAsync_WhenRefreshFails_UsesStaleCache()
    {
        var tenantUrl = "https://example.com/tenant/";
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(GetOrRefreshAsync_WhenRefreshFails_UsesStaleCache)));
        var service = new CacheService(paths);
        await service.WriteAsync(tenantUrl, CacheKind.OpenApi, new CachedContentRecord
        {
            Url = tenantUrl,
            Content = "{\"openapi\":\"3.1.0\"}",
            FetchedAt = DateTimeOffset.UtcNow.AddHours(-2),
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(-5),
        }, CancellationToken.None);

        var result = await service.GetOrRefreshAsync(
            tenantUrl,
            CacheKind.OpenApi,
            new Uri(tenantUrl),
            TimeSpan.FromMinutes(10),
            force: true,
            (_, _) => throw new HttpRequestException("offline"),
            CancellationToken.None);

        Assert.True(result.FromCache);
        Assert.True(result.IsStale);
    }

    private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? etag)
    {
        var response = new HttpResponseMessage(statusCode);
        if (!string.IsNullOrWhiteSpace(etag))
        {
            response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue(etag);
        }

        return response;
    }
}
