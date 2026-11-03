using System.Net;
using System.Text.Json;

namespace OrchardCore.Cli.Tests;

public class WriteOnlyPropertyTests
{
    private const string Document = """
        {"paths":{"/api/widgets":{"post":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"create"},
          "requestBody":{"content":{"application/json":{"schema":{"type":"object","required":["name"],
            "properties":{"name":{"type":"string"},"apiKey":{"type":"string","writeOnly":true}}}}}}}}}}
        """;

    [Fact]
    public async Task DynamicCommand_WriteOnlyProperty_IsNotAnArgument()
    {
        var (app, _, _) = await CreateAppAsync(nameof(DynamicCommand_WriteOnlyProperty_IsNotAnArgument), ["widgets", "create", "--help"]);

        var create = Assert.Single(Assert.Single(app.RootCommand.Subcommands, command => command.Name == "widgets").Subcommands, command => command.Name == "create");
        var options = create.Options.Select(option => option.Name).ToArray();
        Assert.DoesNotContain("--api-key", options);
        Assert.DoesNotContain("--body", options);
        Assert.Contains("--api-key-env", options);
        Assert.Contains("--api-key-file", options);
        Assert.Contains("--api-key-stdin", options);
        Assert.Contains("--body-file", options);
        Assert.Contains("--name", options);
    }

    [Fact]
    public async Task DynamicCommand_WriteOnlyPropertyOnCommandLine_IsRejectedBeforeAnyRequest()
    {
        string[] args = ["widgets", "create", "--name", "demo", "--api-key", "never-print-this", "--output", "none"];
        var (app, handler, _) = await CreateAppAsync(nameof(DynamicCommand_WriteOnlyPropertyOnCommandLine_IsRejectedBeforeAnyRequest), args);
        using var errors = new StringWriter();

        Assert.NotEqual(0, await app.InvokeAsync(args, errors));
        Assert.Null(handler.Body);
    }

    [Fact]
    public async Task DynamicCommand_WriteOnlyPropertyFromFile_IsSent()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var scratch = TestPaths.CreateScratchDirectory(nameof(DynamicCommand_WriteOnlyPropertyFromFile_IsSent));
        var secretFile = Path.Combine(scratch, "secret.txt");
        await File.WriteAllTextAsync(secretFile, "file-secret\n", cancellationToken);
        string[] args = ["widgets", "create", "--name", "demo", "--api-key-file", secretFile, "--output", "none"];
        var (app, handler, _) = await CreateAppAsync(nameof(DynamicCommand_WriteOnlyPropertyFromFile_IsSent), args);

        Assert.Equal(0, await app.InvokeAsync(args));

        using var body = JsonDocument.Parse(handler.Body!);
        Assert.Equal("file-secret", body.RootElement.GetProperty("apiKey").GetString());
    }

    [Fact]
    public void RemoveWriteOnlyProperties_ResponseReturnsThem_RemovesThemAtAnyDepth()
    {
        var operation = Assert.Single(OpenApiCliParser.Parse(Document));
        using var response = JsonDocument.Parse("""{"name":"demo","APIKEY":"never-print-this","items":[{"apiKey":"never-print-this","id":1}]}""");

        var output = CliApplication.RemoveWriteOnlyProperties(response.RootElement, operation).GetRawText();

        Assert.DoesNotContain("never-print-this", output);
        Assert.Contains("demo", output);
        Assert.Contains("\"id\":1", output);
    }

    [Fact]
    public void RemoveWriteOnlyProperties_NoWriteOnlyProperty_ReturnsResponseUnchanged()
    {
        var operation = new OpenApiOperationDefinition { CliMetadata = new OrchardCore.RemoteManagement.CliOperationMetadata(["widgets"], "show") };
        using var response = JsonDocument.Parse("""{"apiKey":"visible"}""");

        Assert.Equal("""{"apiKey":"visible"}""", CliApplication.RemoveWriteOnlyProperties(response.RootElement, operation).GetRawText());
    }

    private static async Task<(CliApplication App, CapturingHandler Handler, CliPaths Paths)> CreateAppAsync(string name, string[] args)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(name));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "site",
            Contexts = [new TenantContextRecord { Name = "site", TenantUrl = "https://cms.example/" }],
        }, cancellationToken);
        await new CacheService(paths).WriteAsync("https://cms.example/", CacheKind.OpenApi, new CachedContentRecord
        {
            Content = Document,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, cancellationToken);
        var handler = new CapturingHandler();
        var client = new HttpClient(handler);
        return (await CliApplication.CreateAsync(args, paths, client, cancellationToken, new UnsupportedCredentialStore()), handler, paths);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }
    }
}
