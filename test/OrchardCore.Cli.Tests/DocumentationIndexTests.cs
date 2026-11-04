using System.Net;

namespace OrchardCore.Cli.Tests;

public class DocumentationIndexTests
{
    private const string DefaultIndex = "https://docs.orchardcore.net/en/latest/search/search_index.json";

    [Theory]
    [InlineData(null, DefaultIndex)]
    [InlineData(" ", DefaultIndex)]
    [InlineData("not a url", DefaultIndex)]
    [InlineData("http://docs.example/search/search_index.json", DefaultIndex)]
    [InlineData("https://docs.example/en/search/search_index.json", "https://docs.example/en/search/search_index.json")]
    [InlineData("http://localhost:8000/search/search_index.json", "http://localhost:8000/search/search_index.json")]
    public void ResolveDocumentationIndexUri_ConfiguredUrl_UsesOnlySecureAddress(string? configured, string expected)
    {
        Assert.Equal(expected, CliApplication.ResolveDocumentationIndexUri(configured).AbsoluteUri);
    }

    [Theory]
    [InlineData(DefaultIndex, "https://docs.orchardcore.net/en/latest/")]
    [InlineData("https://docs.example/4.0/search/search_index.json", "https://docs.example/4.0/")]
    [InlineData("https://docs.example/4.0/index.json", "https://docs.example/4.0/")]
    public void GetDocumentationBaseUri_IndexUri_ReturnsDocumentationRoot(string index, string expected)
    {
        Assert.Equal(expected, CliApplication.GetDocumentationBaseUri(new Uri(index)).AbsoluteUri);
    }

    [Theory]
    [InlineData(null, DefaultIndex)]
    [InlineData("https://docs.example/4.0/search/search_index.json", "https://docs.example/4.0/search/search_index.json")]
    public async Task DocsUpdate_ContextValue_RequestsThatIndexWithoutCredentials(string? configured, string expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(DocsUpdate_ContextValue_RequestsThatIndexWithoutCredentials)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "site",
            Contexts = [new TenantContextRecord { Name = "site", TenantUrl = "https://cms.example/", DocumentationIndexUrl = configured }],
        }, cancellationToken);
        var handler = new IndexHandler();
        using var client = new HttpClient(handler);
        string[] args = ["docs", "update", "--output", "none"];
        var app = await CliApplication.CreateAsync(args, paths, client, cancellationToken, new UnsupportedCredentialStore());

        Assert.Equal(0, await app.InvokeAsync(args));

        var request = Assert.Single(handler.Requests);
        Assert.Equal(expected, request.RequestUri!.AbsoluteUri);
        Assert.Null(request.Headers.Authorization);
    }

    private sealed class IndexHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("""{"docs":[]}""") });
        }
    }
}
