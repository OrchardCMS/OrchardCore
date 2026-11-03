using System.Net;

namespace OrchardCore.Cli.Tests;

public class EnvironmentCredentialsTests
{
    private const string StoredSecret = "stored-secret";
    private const string EnvironmentSecret = "environment-secret";

    [Theory]
    [InlineData("prod", "prod", true, true, 2, true)]
    [InlineData("PROD", "prod", false, false, 1, true)]
    [InlineData("prod", "staging", true, false, 2, false)]
    [InlineData("prod", "staging", true, true, 2, false)]
    [InlineData("prod", "staging", false, false, 1, false)]
    [InlineData(null, "staging", true, true, 2, false)]
    [InlineData(null, "staging", true, false, 2, true)]
    [InlineData(null, "staging", false, false, 1, true)]
    [InlineData(null, "staging", false, true, 1, false)]
    [InlineData(null, "staging", false, false, 2, false)]
    public async Task ApiInvoke_EnvironmentVariables_ReachOnlyTheirContext(string? scopedContext, string selected, bool named, bool hasStoredLogin, int contextCount, bool expected)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(ApiInvoke_EnvironmentVariables_ReachOnlyTheirContext)));
        var credentials = new FileCredentialStore(paths);
        var configuration = new CliConfiguration { CurrentContext = selected };
        configuration.Contexts.Add(CreateContext(selected));
        if (contextCount == 2)
        {
            configuration.Contexts.Add(CreateContext(selected == "prod" ? "staging" : "prod"));
        }

        await new ContextStore(paths).SaveAsync(configuration, cancellationToken);
        if (hasStoredLogin)
        {
            await credentials.SaveAsync(CliApplication.GetCredentialKey(configuration.Contexts[0]), new StoredToken
            {
                ClientId = "stored-client",
                ClientSecret = StoredSecret,
                Issuer = "https://cms.example/",
                ExpiresAt = DateTimeOffset.MinValue,
            }, cancellationToken);
        }

        var handler = new TokenHandler();
        using var client = new HttpClient(handler);
        List<string> args = named ? ["--context", selected] : [];
        args.AddRange(["api", "invoke", "GET", "api/features", "--output", "none"]);
        var variables = new Dictionary<string, string?>
        {
            ["OC_CLIENT_ID"] = "environment-client",
            ["OC_CLIENT_SECRET"] = EnvironmentSecret,
            ["OC_CONTEXT"] = scopedContext,
        };
        var app = await CliApplication.CreateAsync([.. args], paths, client, cancellationToken, credentials, name => variables.GetValueOrDefault(name));

        Assert.Equal(0, await app.InvokeAsync([.. args]));

        Assert.Equal(expected, handler.TokenRequests.Any(body => body.Contains(EnvironmentSecret, StringComparison.Ordinal)));
        Assert.Equal(!expected && hasStoredLogin, handler.TokenRequests.Any(body => body.Contains(StoredSecret, StringComparison.Ordinal)));
    }

    [Fact]
    public async Task ApiInvoke_ContextScopedVariablesWithoutSecret_ReportsMissingVariable()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(ApiInvoke_ContextScopedVariablesWithoutSecret_ReportsMissingVariable)));
        var configuration = new CliConfiguration { CurrentContext = "prod" };
        configuration.Contexts.Add(CreateContext("prod"));
        await new ContextStore(paths).SaveAsync(configuration, cancellationToken);
        var variables = new Dictionary<string, string?> { ["OC_CLIENT_ID"] = "environment-client", ["OC_CONTEXT"] = "prod" };
        using var client = new HttpClient(new TokenHandler());
        string[] args = ["api", "invoke", "GET", "api/features", "--output", "none"];
        using var errors = new StringWriter();
        var app = await CliApplication.CreateAsync(args, paths, client, cancellationToken, new FileCredentialStore(paths), name => variables.GetValueOrDefault(name));

        Assert.Equal(1, await app.InvokeAsync(args, errors));
        Assert.Contains("OC_CLIENT_SECRET", errors.ToString());
    }

    [Theory]
    [InlineData(null, false, true, 2, true)]
    [InlineData("", false, true, 2, true)]
    [InlineData(" ", false, false, 1, true)]
    [InlineData("other", false, true, 1, false)]
    public void AppliesEnvironmentCredentials_BlankScope_FollowsStoredLoginAndSelection(string? scopedContext, bool hasStoredCredentials, bool isNamed, int contextCount, bool expected)
    {
        Assert.Equal(expected, CliApplication.AppliesEnvironmentCredentials("prod", scopedContext, hasStoredCredentials, isNamed, contextCount));
    }

    private static TenantContextRecord CreateContext(string name) => new()
    {
        Name = name,
        TenantUrl = "https://cms.example/",
        Authority = "https://cms.example/",
        ClientId = "orchardcore-cli",
    };

    private sealed class TokenHandler : HttpMessageHandler
    {
        public List<string> TokenRequests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            var response = "[]";
            if (path.EndsWith("openid-configuration", StringComparison.Ordinal))
            {
                response = """{"issuer":"https://cms.example/","token_endpoint":"https://cms.example/connect/token"}""";
            }
            else if (path.EndsWith("connect/token", StringComparison.Ordinal))
            {
                TokenRequests.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
                response = """{"access_token":"token","token_type":"Bearer","expires_in":3600}""";
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) };
        }
    }
}
