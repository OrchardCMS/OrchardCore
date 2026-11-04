using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.Json;

namespace OrchardCore.Cli.Tests;

public class SecretResponseTests
{
    [Fact]
    public async Task SecretResponse_RequiresPrivateDestinationBeforeSendingMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SecretResponse_RequiresPrivateDestinationBeforeSendingMutation)));
        var handler = new RotationHandler();
        using var client = new HttpClient(handler);
        string[] args = ["credentials", "rotate", "--force", "--output", "none"];
        var app = await CreateApplicationAsync(args, paths, client, cancellationToken);
        Assert.NotEqual(0, await app.InvokeAsync(args));
        Assert.Equal(0, handler.Mutations);
    }

    [Fact]
    public async Task SecretResponse_WritesOnce_AndRefusesExistingFileBeforeMutation()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(SecretResponse_WritesOnce_AndRefusesExistingFileBeforeMutation)));
        var destination = Path.Combine(paths.RootDirectory, "rotation.json");
        var handler = new RotationHandler();
        using var client = new HttpClient(handler);
        string[] args = ["credentials", "rotate", "--force", "--secret-output-file", destination, "--output", "none"];
        var app = await CreateApplicationAsync(args, paths, client, cancellationToken);
        Assert.Equal(0, await app.InvokeAsync(args));
        Assert.Equal(1, handler.Mutations);
        var saved = await File.ReadAllTextAsync(destination, cancellationToken);
        using var response = JsonDocument.Parse(saved);
        Assert.True(response.RootElement.GetProperty("clientSecret").GetString() == "synthetic-one-time-value");
        Assert.NotEqual(0, await app.InvokeAsync(args));
        Assert.Equal(1, handler.Mutations);
        Assert.True(saved == await File.ReadAllTextAsync(destination, cancellationToken));
    }

    [Fact]
    public async Task PrivateOutput_PreservesResponseButReturnsOnlyDestination()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var directory = TestPaths.CreateScratchDirectory(nameof(PrivateOutput_PreservesResponseButReturnsOnlyDestination));
        var destination = new FileInfo(Path.Combine(directory, "secret.json"));
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        using var response = JsonDocument.Parse(JsonSerializer.Serialize(new { clientSecret = secret, unexpectedSecret = secret }));
        await using (var file = SecretOutputFile.Create(destination))
        {
            var output = await file.WriteAsync(response.RootElement, cancellationToken);
            Assert.Equal(destination.FullName, output.GetProperty("secretOutputFile").GetString());
            Assert.Single(output.EnumerateObject());
            Assert.False(output.GetRawText().Contains(secret, StringComparison.Ordinal));
        }
        using var saved = JsonDocument.Parse(await File.ReadAllTextAsync(destination.FullName, cancellationToken));
        Assert.True(saved.RootElement.GetProperty("clientSecret").GetString() == secret);
        Assert.True(saved.RootElement.GetProperty("unexpectedSecret").GetString() == secret);
        if (OperatingSystem.IsWindows())
        {
            var security = destination.GetAccessControl();
            var user = WindowsIdentity.GetCurrent().User;
            Assert.True(security.AreAccessRulesProtected);
            Assert.Equal(user, security.GetOwner(typeof(SecurityIdentifier)));
            foreach (FileSystemAccessRule rule in security.GetAccessRules(true, true, typeof(SecurityIdentifier)))
            {
                Assert.Equal(user, rule.IdentityReference);
            }
        }
        else
        {
            Assert.Equal(UnixFileMode.UserRead | UnixFileMode.UserWrite, File.GetUnixFileMode(destination.FullName));
        }
    }

    [Fact]
    public async Task PrivateOutput_CleansUnusedReservation()
    {
        var path = Path.Combine(TestPaths.CreateScratchDirectory(nameof(PrivateOutput_CleansUnusedReservation)), "secret.json");
        await using (SecretOutputFile.Create(new FileInfo(path)))
        {
            Assert.True(File.Exists(path));
        }
        Assert.False(File.Exists(path));
    }

    private static async Task<CliApplication> CreateApplicationAsync(string[] args, CliPaths paths, HttpClient client, CancellationToken cancellationToken)
    {
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "site",
            Contexts = [new TenantContextRecord { Name = "site", TenantUrl = "https://cms.example/" }],
        }, cancellationToken);
        await new CacheService(paths).WriteAsync("https://cms.example/", CacheKind.OpenApi, new CachedContentRecord
        {
            Content = """
            {"paths":{"/api/credentials:rotate":{"post":{
              "operationId":"RotateCredential",
              "x-oc-cli":{"commandGroup":["credentials"],"verb":"rotate","secretResponse":true,"requiresConfirmation":true}
            }}}}
            """,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, cancellationToken);
        return await CliApplication.CreateAsync(args, paths, client, cancellationToken, new FileCredentialStore(paths));
    }

    private sealed class RotationHandler : HttpMessageHandler
    {
        public int Mutations { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.Equal("/api/credentials:rotate", request.RequestUri!.AbsolutePath);
            Mutations++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"clientId":"automation","clientSecret":"synthetic-one-time-value"}"""),
            });
        }
    }
}
