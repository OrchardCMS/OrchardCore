using System.Net;
using System.Text.Json;

namespace OrchardCore.Cli.Tests;

public class ClientProvisioningTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task EnableRemoteManagement_OptIn_SavesSeparateContextAndRenewsApplicationTokens(bool provision, bool install)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(EnableRemoteManagement_OptIn_SavesSeparateContextAndRenewsApplicationTokens)));
        var store = new ContextStore(paths);
        var credentials = new FileCredentialStore(paths);
        await store.SaveAsync(new CliConfiguration
        {
            CurrentContext = "child",
            Contexts = [new TenantContextRecord { Name = "child", TenantUrl = "https://cms.example/" }],
        }, cancellationToken);
        await new CacheService(paths).WriteAsync("https://cms.example/", CacheKind.OpenApi, new CachedContentRecord
        {
            Content = """
            {"paths":{"/api/tenants/{tenantName}:enable-remote-management":{"post":{
              "operationId":"ApiEnableTenantRemoteManagement",
              "parameters":[{"name":"tenantName","in":"path","required":true,"schema":{"type":"string"}},
                            {"name":"provisionClient","in":"query","schema":{"type":"boolean"}}],
              "x-oc-cli":{"commandGroup":["tenants"],"verb":"enable-remote-management","arguments":[{"parameterName":"tenantName","position":0}]}
            }},"/api/tenants/{tenantName}:install":{"post":{
              "operationId":"ApiInstallTenantManagement",
              "parameters":[{"name":"tenantName","in":"path","required":true,"schema":{"type":"string"}}],
              "requestBody":{"content":{"application/json":{"schema":{"type":"object","properties":{"enableRemoteManagement":{"type":"boolean"}}}}}},
              "x-oc-cli":{"commandGroup":["tenants"],"verb":"install","inputMode":"options","arguments":[{"parameterName":"tenantName","position":0}]}
            }}}}
            """,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, cancellationToken);
        var handler = new ProvisioningHandler(provision);
        using var client = new HttpClient(handler);
        var args = new List<string> { "tenants", install ? "install" : "enable-remote-management", "child", "--output", "none" };
        if (provision)
        {
            args.Add(install ? "--enable-remote-management" : "--provision-client");
        }

        var app = await CliApplication.CreateAsync([.. args], paths, client, cancellationToken, credentials);
        Assert.Equal(0, await app.InvokeAsync([.. args]));
        var configuration = await store.LoadAsync(cancellationToken);
        Assert.Equal("child", configuration.CurrentContext);
        Assert.Equal(provision ? 2 : 1, configuration.Contexts.Count);
        Assert.DoesNotContain("application-secret", await File.ReadAllTextAsync(paths.ConfigFilePath, cancellationToken));
        if (!provision)
        {
            return;
        }

        var context = Assert.Single(configuration.Contexts, value => value.Name == "child-2");
        Assert.Equal("https://cms.example/child/", context.TenantUrl);
        var key = CliApplication.GetCredentialKey(context);
        Assert.Equal("application-secret", (await credentials.GetAsync(key, cancellationToken))!.ClientSecret);
        string[] invoke = ["--context", context.Name, "api", "invoke", "GET", "api/features", "--output", "none"];
        for (var index = 0; index < 2; index++)
        {
            var next = await CliApplication.CreateAsync(invoke, paths, client, cancellationToken, credentials);
            Assert.Equal(0, await next.InvokeAsync(invoke));
            var saved = await credentials.GetAsync(key, cancellationToken);
            Assert.Equal("application-secret", saved!.ClientSecret);
            saved.ExpiresAt = DateTimeOffset.MinValue;
            await credentials.SaveAsync(key, saved, cancellationToken);
        }

        Assert.Equal(2, handler.TokenRequests);
        string[] logout = ["--context", context.Name, "logout", "--output", "none"];
        var logoutApp = await CliApplication.CreateAsync(logout, paths, client, cancellationToken, credentials);
        Assert.Equal(0, await logoutApp.InvokeAsync(logout));
        Assert.Null(await credentials.GetAsync(key, cancellationToken));
    }

    [Fact]
    public async Task ProvisionedResponse_RemovesSecretAndReturnsUsableContextName()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(ProvisionedResponse_RemovesSecretAndReturnsUsableContextName)));
        using var client = new HttpClient();
        var app = await CliApplication.CreateAsync(["context", "list"], paths, client, TestContext.Current.CancellationToken, new FileCredentialStore(paths));
        using var document = JsonDocument.Parse("""{"name":"child","primaryUrl":"https://cms.example/child","clientCredentials":{"clientId":"pomi-test","clientSecret":"never-print-this"}}""");
        var output = await app.CaptureProvisionedContextAsync(document.RootElement, TestContext.Current.CancellationToken);
        Assert.False(output.TryGetProperty("clientCredentials", out _));
        Assert.DoesNotContain("never-print-this", output.GetRawText());
        Assert.Equal("child", output.GetProperty("context").GetString());
        Assert.Equal("pomi-test", output.GetProperty("clientId").GetString());
        var next = NextStepFormatter.Format(new CommandOutput { Json = output, CommandPath = ["tenants", "install"] });
        Assert.Contains("--context=child", next);
        Assert.DoesNotContain("login", next);
    }

    private sealed class ProvisioningHandler : HttpMessageHandler
    {
        private readonly bool _provision;
        public int TokenRequests { get; private set; }

        public ProvisioningHandler(bool provision)
        {
            _provision = provision;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;
            string response;
            if (path.EndsWith(":enable-remote-management", StringComparison.Ordinal) || path.EndsWith(":install", StringComparison.Ordinal))
            {
                if (path.EndsWith(":install", StringComparison.Ordinal))
                {
                    var body = request.Content is null ? "{}" : await request.Content.ReadAsStringAsync(cancellationToken);
                    using var document = JsonDocument.Parse(body);
                    Assert.Equal(_provision, document.RootElement.TryGetProperty("enableRemoteManagement", out var enabled) && enabled.GetBoolean());
                }
                else
                {
                    Assert.Equal(_provision ? "?provisionClient=true" : string.Empty, request.RequestUri.Query);
                }
                response = _provision
                    ? """{"name":"child","state":"Running","url":"https://cms.example/child","clientCredentials":{"clientId":"pomi-test","clientSecret":"application-secret"}}"""
                    : """{"name":"child","state":"Running","url":"https://cms.example/child"}""";
            }
            else if (path.EndsWith("orchardcore-management", StringComparison.Ordinal))
            {
                response = """{"protocolMajorVersion":1,"managementManifestUrl":"https://cms.example/child/api/management/manifest","authentication":{"authority":"https://cms.example/child/","clientId":"orchardcore-cli","grantTypes":["client_credentials"],"scopes":["orchardcore.management"]}}""";
            }
            else if (path.EndsWith("openid-configuration", StringComparison.Ordinal))
            {
                response = """{"issuer":"https://cms.example/child/","token_endpoint":"https://cms.example/child/connect/token"}""";
            }
            else if (path.EndsWith("connect/token", StringComparison.Ordinal))
            {
                var body = await request.Content!.ReadAsStringAsync(cancellationToken);
                Assert.Contains("grant_type=client_credentials", body);
                Assert.Contains("client_id=pomi-test", body);
                Assert.Contains("client_secret=application-secret", body);
                Assert.DoesNotContain("refresh_token", body);
                TokenRequests++;
                response = """{"access_token":"application-token","token_type":"Bearer","expires_in":3600}""";
            }
            else
            {
                Assert.Equal("/child/api/features", path);
                Assert.Equal("application-token", request.Headers.Authorization!.Parameter);
                response = "[]";
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(response) };
        }
    }
}
