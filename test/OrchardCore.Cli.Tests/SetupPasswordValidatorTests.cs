using System.Net;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Cli.Tests;

public class SetupPasswordValidatorTests
{
    [Theory]
    [InlineData("")]
    [InlineData("Aa1!")]
    [InlineData("lowercase1!")]
    [InlineData("UPPERCASE1!")]
    [InlineData("NoDigitsHere!")]
    [InlineData("NoSymbols123")]
    [InlineData("Élowercase1!")]
    public void Validate_InvalidPassword_RejectsWithoutEchoingSecret(string password)
    {
        var error = Assert.Throws<CliException>(() => SetupPasswordValidator.Validate(password));
        Assert.Contains("at least 6", error.Message);
        if (password.Length > 0)
        {
            Assert.DoesNotContain(password, error.Message);
        }
    }

    [Theory]
    [InlineData("Aa1!xx")]
    [InlineData("Longer-Password123!")]
    public void Validate_ValidPassword_Accepts(string password) => SetupPasswordValidator.Validate(password);

    [Fact]
    public void LocalInstall_InvalidPassword_DoesNotCreateDestination()
    {
        var destination = Path.Combine(TestPaths.CreateScratchDirectory(nameof(LocalInstall_InvalidPassword_DoesNotCreateDestination)), "site");
        var options = new LocalSiteInstallOptions
        {
            Directory = destination, SiteName = "Site", UserName = "admin", Email = "admin@example.com", Password = "weak-password",
        };
        Assert.Throws<CliException>(() => LocalSiteInstaller.ValidateSecrets(options));
        Assert.False(Directory.Exists(destination));
    }

    [Theory]
    [InlineData("install", "file")]
    [InlineData("install", "env")]
    [InlineData("install", "body-file")]
    [InlineData("setup", "file")]
    [InlineData("setup", "env")]
    [InlineData("setup", "body-file")]
    public async Task TenantSetup_InvalidPassword_DoesNotSendRequest(string verb, string input)
    {
        const string tenant = "https://example.com/";
        const string secret = "missing-uppercase1!";
        var directory = TestPaths.CreateScratchDirectory(nameof(TenantSetup_InvalidPassword_DoesNotSendRequest));
        var paths = new CliPaths(directory);
        var cancellationToken = TestContext.Current.CancellationToken;
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "test",
            Contexts = [new TenantContextRecord { Name = "test", TenantUrl = tenant }],
        }, cancellationToken);
        var document = """
            {"paths":{"/api/tenants/{tenantName}:__VERB__":{"post":{
              "parameters":[{"name":"tenantName","in":"path","required":true,"schema":{"type":"string"}}],
              "requestBody":{"required":true,"content":{"application/json":{"schema":{
                "type":"object","required":["password"],"properties":{"password":{"type":"string"}}
              }}}},
              "x-oc-cli":{"commandGroup":["tenants"],"verb":"__VERB__","secretProperties":["password"],"arguments":[{"parameterName":"tenantName","position":0}]}
            }}}}
            """.Replace("__VERB__", verb, StringComparison.Ordinal);
        await new CacheService(paths).WriteAsync(tenant, CacheKind.OpenApi, new CachedContentRecord
        {
            Content = document, ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(10),
        }, cancellationToken);
        var secretFile = Path.Combine(directory, "secret.txt");
        await File.WriteAllTextAsync(secretFile, input == "body-file" ? $$"""{"password":"{{secret}}"}""" : secret, cancellationToken);
        var variable = "POMI_TEST_PASSWORD_" + Guid.NewGuid().ToString("N");
        System.Environment.SetEnvironmentVariable(variable, secret);
        try
        {
            using var handler = new CountingHandler();
            using var http = new HttpClient(handler);
            var app = await CliApplication.CreateAsync(["tenants", verb, "--help"], paths, http, cancellationToken, new UnsupportedCredentialStore());
            string[] args = ["tenants", verb, "NewTenant", input == "body-file" ? "--body-file" : "--password-" + input, input == "env" ? variable : secretFile];
            using var errors = new StringWriter();
            Assert.NotEqual(0, await app.InvokeAsync(args, errors));
            Assert.Contains("uppercase letter", errors.ToString());
            Assert.DoesNotContain(secret, errors.ToString());
            Assert.Equal(0, handler.RequestCount);
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(variable, null);
        }
    }

    [Fact]
    public void OtherOperations_DoNotApplySetupPolicy()
    {
        CliApplication.ValidateDynamicJsonBody("""{"password":"unchanged"}""", new OpenApiOperationDefinition
        {
            Method = "POST", CliMetadata = new CliOperationMetadata(["users"], "create"),
        });
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
