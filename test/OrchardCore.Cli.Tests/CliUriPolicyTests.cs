using System.Net;

namespace OrchardCore.Cli.Tests;

public class CliUriPolicyTests
{
    [Theory]
    [InlineData("http://cms.example.com/")]
    [InlineData("file:///tmp/site")]
    [InlineData("https://user:password@cms.example.com/")]
    [InlineData("https://cms.example.com/#fragment")]
    public void NormalizeTenantUrl_UntrustedTransport_Rejects(string url)
    {
        Assert.Throws<CliException>(() => CliPaths.NormalizeTenantUrl(url));
    }

    [Theory]
    [InlineData("https://cms.example.com/site/", "/api/items", "https://cms.example.com/site/api/items")]
    [InlineData("http://127.0.0.1:5000/site/", "api/items", "http://127.0.0.1:5000/site/api/items")]
    public void BuildRequestUri_TenantRelativePath_PreservesPathBase(string tenant, string path, string expected)
    {
        Assert.Equal(expected, CliApplication.BuildRequestUri(new Uri(tenant), path, new Dictionary<string, string>()).AbsoluteUri);
    }

    [Theory]
    [InlineData("https://attacker.example/api/items")]
    [InlineData("../api/items")]
    [InlineData("https://cms.example.com/other/api/items")]
    [InlineData("https://cms.example.com/site-other/api/items")]
    [InlineData("api/%2f..%2f..%2fother")]
    [InlineData("api/%252e%252e/other")]
    [InlineData("api/%5c..%5cother")]
    public void BuildRequestUri_EscapesTenant_Rejects(string path)
    {
        Assert.Throws<CliException>(() => CliApplication.BuildRequestUri(new Uri("https://cms.example.com/site/"), path, new Dictionary<string, string>()));
    }

    [Fact]
    public void GetCredentialKey_SameNameDifferentTenantOrClient_DoesNotShareTokens()
    {
        var context = new TenantContextRecord { Name = "production", TenantUrl = "https://cms.example.com/a/", Authority = "https://cms.example.com/a/", ClientId = "pomi" };
        var original = CliApplication.GetCredentialKey(context);
        context.Name = "PRODUCTION";
        Assert.Equal(original, CliApplication.GetCredentialKey(context));
        context.TenantUrl = "https://cms.example.com/b/";
        Assert.NotEqual(original, CliApplication.GetCredentialKey(context));
        context.TenantUrl = "https://cms.example.com/a/";
        context.ClientId = "another-client";
        Assert.NotEqual(original, CliApplication.GetCredentialKey(context));
    }

    [Theory]
    [InlineData("http://example.com/connect/token")]
    [InlineData("https://attacker.example/connect/token")]
    public async Task GetDiscoveryAsync_UntrustedTokenEndpoint_RejectsBeforeSendingCredentials(string endpoint)
    {
        using var client = new HttpClient(new DiscoveryHandler(endpoint));
        var oauth = new OAuthClient(client, TextWriter.Null);
        await Assert.ThrowsAsync<CliException>(() => oauth.GetDiscoveryAsync(new Uri("https://example.com/"), TestContext.Current.CancellationToken));
    }

    private sealed class DiscoveryHandler : HttpMessageHandler
    {
        private readonly string _endpoint;

        public DiscoveryHandler(string endpoint) => _endpoint = endpoint;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent($$"""{"issuer":"https://example.com/","token_endpoint":"{{_endpoint}}"}"""),
        });
    }
}
