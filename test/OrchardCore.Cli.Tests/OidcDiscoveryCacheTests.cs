using System.Net;

namespace OrchardCore.Cli.Tests;

public class OidcDiscoveryCacheTests
{
    private const string Document = """{"issuer":"https://cms.example/","token_endpoint":"https://cms.example/connect/token"}""";
    private static readonly Uri _authority = new("https://cms.example/");

    [Fact]
    public async Task GetDiscoveryAsync_WithinTtl_RequestsDocumentOnce()
    {
        var (handler, client, cache) = Create(nameof(GetDiscoveryAsync_WithinTtl_RequestsDocumentOnce));

        var first = await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        var second = await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.DiscoveryRequests);
        Assert.Equal(first.TokenEndpoint, second.TokenEndpoint);
        var record = await cache.ReadAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, TestContext.Current.CancellationToken);
        Assert.InRange(record!.ExpiresAt, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMinutes(10));
    }

    [Fact]
    public async Task GetDiscoveryAsync_NoCache_RequestsDocumentEveryTime()
    {
        var handler = new DiscoveryHandler();
        var client = new OAuthClient(new HttpClient(handler), TextWriter.Null);

        await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.DiscoveryRequests);
    }

    [Fact]
    public async Task GetDiscoveryAsync_Refresh_ReplacesCachedDocument()
    {
        var (handler, client, _) = Create(nameof(GetDiscoveryAsync_Refresh_ReplacesCachedDocument));
        await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        handler.Response = """{"issuer":"https://cms.example/","token_endpoint":"https://cms.example/moved/token"}""";

        Assert.Equal("https://cms.example/moved/token", (await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken, refresh: true)).TokenEndpoint);
        Assert.Equal("https://cms.example/moved/token", (await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken)).TokenEndpoint);
        Assert.Equal(2, handler.DiscoveryRequests);
    }

    [Fact]
    public async Task GetDiscoveryAsync_Expired_RequestsDocumentAgain()
    {
        var (handler, client, cache) = Create(nameof(GetDiscoveryAsync_Expired_RequestsDocumentAgain));
        await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        var record = await cache.ReadAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, TestContext.Current.CancellationToken);
        record!.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        await cache.WriteAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, record, TestContext.Current.CancellationToken);

        await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.DiscoveryRequests);
    }

    [Fact]
    public async Task GetDiscoveryAsync_RequestFails_RemovesExpiredDocumentAndThrows()
    {
        var (handler, client, cache) = Create(nameof(GetDiscoveryAsync_RequestFails_RemovesExpiredDocumentAndThrows));
        await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        var record = await cache.ReadAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, TestContext.Current.CancellationToken);
        record!.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        await cache.WriteAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, record, TestContext.Current.CancellationToken);
        handler.Status = HttpStatusCode.ServiceUnavailable;

        await Assert.ThrowsAsync<HttpRequestException>(() => client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken));

        Assert.Null(await cache.ReadAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task GetDiscoveryAsync_CachedDocumentNoLongerTrusted_RequestsItAgain()
    {
        var (handler, client, cache) = Create(nameof(GetDiscoveryAsync_CachedDocumentNoLongerTrusted_RequestsItAgain));
        await cache.WriteAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, new CachedContentRecord
        {
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            Content = """{"issuer":"https://cms.example/","token_endpoint":"https://attacker.example/connect/token"}""",
        }, TestContext.Current.CancellationToken);

        var discovery = await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);

        Assert.Equal("https://cms.example/connect/token", discovery.TokenEndpoint);
        Assert.Equal(1, handler.DiscoveryRequests);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, true)]
    [InlineData(HttpStatusCode.InternalServerError, true)]
    [InlineData(HttpStatusCode.BadRequest, false)]
    public async Task ClientCredentialsAsync_TokenEndpointResponse_RemovesCachedDocumentOnlyWhenEndpointFails(HttpStatusCode status, bool removed)
    {
        var (handler, client, cache) = Create(nameof(ClientCredentialsAsync_TokenEndpointResponse_RemovesCachedDocumentOnlyWhenEndpointFails));
        var discovery = await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        handler.TokenStatus = status;

        await Assert.ThrowsAnyAsync<Exception>(() => client.ClientCredentialsAsync(
            discovery.TokenEndpoint, "client", "secret", "orchardcore.management", discovery.Issuer, TestContext.Current.CancellationToken));

        Assert.Equal(removed, await cache.ReadAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, TestContext.Current.CancellationToken) is null);
    }

    [Fact]
    public async Task ClientCredentialsAsync_TokenEndpointUnreachable_RemovesCachedDocument()
    {
        var (handler, client, cache) = Create(nameof(ClientCredentialsAsync_TokenEndpointUnreachable_RemovesCachedDocument));
        var discovery = await client.GetDiscoveryAsync(_authority, TestContext.Current.CancellationToken);
        handler.TokenFailure = new HttpRequestException("Connection refused.");

        await Assert.ThrowsAsync<HttpRequestException>(() => client.ClientCredentialsAsync(
            discovery.TokenEndpoint, "client", "secret", "orchardcore.management", discovery.Issuer, TestContext.Current.CancellationToken));

        Assert.Null(await cache.ReadAsync(_authority.AbsoluteUri, CacheKind.OidcDiscovery, TestContext.Current.CancellationToken));
    }

    private static (DiscoveryHandler Handler, OAuthClient Client, CacheService Cache) Create(string name)
    {
        var handler = new DiscoveryHandler();
        var cache = new CacheService(new CliPaths(TestPaths.CreateScratchDirectory(name)));
        return (handler, new OAuthClient(new HttpClient(handler), TextWriter.Null, cache: cache), cache);
    }

    private sealed class DiscoveryHandler : HttpMessageHandler
    {
        public int DiscoveryRequests { get; private set; }

        public string Response { get; set; } = Document;

        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        public HttpStatusCode TokenStatus { get; set; } = HttpStatusCode.OK;

        public Exception? TokenFailure { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("openid-configuration", StringComparison.Ordinal))
            {
                DiscoveryRequests++;
                return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(Response) });
            }

            if (TokenFailure is not null)
            {
                throw TokenFailure;
            }

            return Task.FromResult(new HttpResponseMessage(TokenStatus) { Content = new StringContent("""{"access_token":"token","token_type":"Bearer"}""") });
        }
    }
}
