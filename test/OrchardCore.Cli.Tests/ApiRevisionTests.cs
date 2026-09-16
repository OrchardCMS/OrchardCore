using System.Net;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class ApiRevisionTests
{
    private const string Tenant = "https://example.com/blog/";
    private static readonly string _oldRevision = new('a', 64);
    private static readonly string _newRevision = new('b', 64);
    private const string OldDocument = """{"paths":{"/api/old":{"get":{"operationId":"Old","x-oc-cli":{"commandGroup":["old"],"verb":"list"}},"post":{"operationId":"Change","x-oc-cli":{"commandGroup":["old"],"verb":"change"}}}}}""";
    private const string NewDocument = """{"paths":{"/api/new":{"get":{"operationId":"New","x-oc-cli":{"commandGroup":["new"],"verb":"list"}}}}}""";
    private const string Manifest = """{"protocolMajorVersion":1,"authentication":{"authority":"https://example.com/blog/"},"managementManifestUrl":"https://example.com/blog/api/management/manifest","openApiUrl":"https://example.com/blog/openapi/v1.json"}""";

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateAsync_RevisionProbe_RefreshesOnlyWhenChanged(bool changed)
    {
        var paths = await SeedAsync();
        using var handler = new RevisionHandler(changed ? _newRevision : _oldRevision);
        using var client = new HttpClient(handler);
        var app = await CliApplication.CreateAsync([changed ? "new" : "old", "list"], paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Contains(app.RootCommand.Subcommands, command => command.Name == (changed ? "new" : "old"));
        Assert.DoesNotContain(app.RootCommand.Subcommands, command => command.Name == (changed ? "old" : "new"));
        Assert.Equal(changed ? 3 : 1, handler.Requests.Count);
        Assert.Equal("HEAD /blog/api/management/manifest", handler.Requests[0]);
        var cached = await new CacheService(paths).ReadAsync(Tenant, CacheKind.OpenApi, TestContext.Current.CancellationToken);
        Assert.Equal(changed ? _newRevision : _oldRevision, cached!.ApiRevision);
        Assert.True(cached.ExpiresAt > DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("[suggest:0]")]
    public async Task CreateAsync_OfflineMetadata_DoesNotProbe(string argument)
    {
        var paths = await SeedAsync();
        using var handler = new RevisionHandler(_newRevision);
        using var client = new HttpClient(handler);
        _ = await CliApplication.CreateAsync([argument], paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        _ = await CliApplication.CreateAsync([], paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Empty(handler.Requests);
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "list")]
    [InlineData(HttpStatusCode.NotFound, "list")]
    [InlineData(HttpStatusCode.OK, "change")]
    [InlineData(HttpStatusCode.NotFound, "change")]
    public async Task InvokeAsync_RevisionChangesDuringOperation_InvalidatesWithoutReplaying(HttpStatusCode status, string verb)
    {
        var paths = await SeedAsync();
        using var handler = new RevisionHandler(_oldRevision, status);
        using var client = new HttpClient(handler);
        var args = new[] { "old", verb, "--output", "none" };
        var app = await CliApplication.CreateAsync(args, paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        using var errors = new StringWriter();

        Assert.Equal(status == HttpStatusCode.OK ? 0 : 4, await app.InvokeAsync(args, errors));
        Assert.Equal(2, handler.Requests.Count);
        foreach (var kind in new[] { CacheKind.Manifest, CacheKind.OpenApi })
        {
            var cached = await new CacheService(paths).ReadAsync(Tenant, kind, TestContext.Current.CancellationToken);
            Assert.Equal(DateTimeOffset.MinValue, cached!.ExpiresAt);
        }
    }

    [Fact]
    public async Task CreateAsync_ProbeFails_KeepsFreshCommandsForNormalExecution()
    {
        var paths = await SeedAsync();
        using var handler = new RevisionHandler(_oldRevision) { FailProbe = true };
        using var client = new HttpClient(handler);
        var args = new[] { "old", "list", "--output", "none" };
        var app = await CliApplication.CreateAsync(args, paths, client, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Equal(0, await app.InvokeAsync(args));
        Assert.Equal(2, handler.Requests.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    [InlineData("abcd")]
    public async Task ObserveApiRevisionAsync_MissingOrMalformedHeader_KeepsCache(string? value)
    {
        var paths = await SeedAsync();
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        if (value is not null)
        {
            response.Headers.Add(RemoteManagementConstants.ApiRevisionHeaderName, value);
        }

        Assert.False(await new CacheService(paths).ObserveApiRevisionAsync(Tenant, response, TestContext.Current.CancellationToken));
        var cached = await new CacheService(paths).ReadAsync(Tenant, CacheKind.OpenApi, TestContext.Current.CancellationToken);
        Assert.True(cached!.ExpiresAt > DateTimeOffset.UtcNow);
    }

    private static async Task<CliPaths> SeedAsync()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(ApiRevisionTests)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "test",
            Contexts = [new TenantContextRecord { Name = "test", TenantUrl = Tenant }],
        }, TestContext.Current.CancellationToken);
        foreach (var kind in new[] { CacheKind.Manifest, CacheKind.OpenApi })
        {
            await new CacheService(paths).WriteAsync(Tenant, kind, new CachedContentRecord
            {
                Content = kind == CacheKind.Manifest ? Manifest : OldDocument,
                ApiRevision = _oldRevision,
                ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(30),
            }, TestContext.Current.CancellationToken);
        }

        return paths;
    }

    private sealed class RevisionHandler : HttpMessageHandler
    {
        private readonly string _revision;
        private readonly HttpStatusCode _operationStatus;
        public List<string> Requests { get; } = [];
        public bool FailProbe { get; init; }

        public RevisionHandler(string revision, HttpStatusCode operationStatus = HttpStatusCode.OK)
        {
            _revision = revision;
            _operationStatus = operationStatus;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            Requests.Add($"{request.Method} {path}");
            if (FailProbe && request.Method == HttpMethod.Head)
            {
                throw new HttpRequestException("Probe unavailable");
            }

            var operation = path == "/blog/api/old";
            var content = request.Method == HttpMethod.Head ? "" : path switch
            {
                "/blog/api/management/manifest" => Manifest,
                "/blog/openapi/v1.json" => NewDocument,
                "/blog/api/old" => "{}",
                _ => throw new InvalidOperationException($"Unexpected request: {request.Method} {path}"),
            };
            var response = new HttpResponseMessage(operation ? _operationStatus : HttpStatusCode.OK)
            {
                Content = new StringContent(content),
            };
            response.Headers.Add(RemoteManagementConstants.ApiRevisionHeaderName, operation ? _newRevision : _revision);
            return Task.FromResult(response);
        }
    }
}
