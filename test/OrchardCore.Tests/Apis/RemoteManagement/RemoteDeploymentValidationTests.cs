using OrchardCore.Deployment.Remote.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class RemoteDeploymentValidationTests
{
    [Theory]
    [InlineData("https://example.org/deploy", true)]
    [InlineData("http://localhost/deploy", true)]
    [InlineData("http://127.0.0.1/deploy", true)]
    [InlineData("http://example.org/deploy", false)]
    [InlineData("file:///tmp/deploy", false)]
    [InlineData("https://user:password@example.org/deploy", false)]
    [InlineData("https://example.org/deploy#fragment", false)]
    [InlineData("/relative", false)]
    public void Validate_TransportBoundary_ProtectsCredentials(string url, bool allowed)
    {
        Assert.Equal(allowed, RemoteDeploymentValidation.IsSafeUrl(url));
        Assert.Equal(allowed, RemoteDeploymentValidation.Instance("target", url, "client", "key").Count == 0);
    }

    [Fact]
    public void Validate_MissingOrOversizedValues_RejectsConfiguration()
    {
        Assert.Equal(4, RemoteDeploymentValidation.Instance("", "", "", "").Count);
        Assert.Equal(2, RemoteDeploymentValidation.Client(new string('n', 257), new string('k', 4097)).Count);
    }

    [Fact]
    public void HttpClient_Redirects_DisabledForCredentialBearingRequests()
    {
        var services = new ServiceCollection();
        var startup = (global::OrchardCore.Modules.StartupBase)Activator.CreateInstance(typeof(RemoteDeploymentSender).Assembly.GetType("OrchardCore.Deployment.Startup"));
        startup.ConfigureServices(services);
        using var provider = services.BuildServiceProvider();
        var handler = provider.GetRequiredService<IHttpMessageHandlerFactory>().CreateHandler(RemoteDeploymentSender.HttpClientName);
        while (handler is DelegatingHandler delegating) { handler = delegating.InnerHandler; }
        Assert.False(Assert.IsType<HttpClientHandler>(handler).AllowAutoRedirect);
    }
}
