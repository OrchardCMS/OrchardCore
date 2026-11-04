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

    [Theory]
    [InlineData("""{"password":"never-send-this"}""")]
    [InlineData("""{"PASSWORD":"never-send-this"}""")]
    [InlineData("""{"nested":{"password":"never-send-this"}}""")]
    public async Task SectionCommand_WriteOnlyValueInInlineBody_IsRefusedBeforeUpdating(string body)
    {
        string[] args = ["sections", "update", "smtp", "--body", body, "--output", "json"];
        var (app, handler, _) = await CreateAppAsync(nameof(SectionCommand_WriteOnlyValueInInlineBody_IsRefusedBeforeUpdating), args, SectionDocument, SectionSchema);
        using var errors = new StringWriter();

        Assert.Equal(1, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));

        Assert.Contains("--body-file", errors.ToString());
        Assert.DoesNotContain("never-send-this", errors.ToString());
        Assert.Equal(["GET /api/sections/smtp/schema"], handler.Requests);
    }

    [Fact]
    public async Task SectionCommand_OtherValuesInInlineBody_UpdatesTheSection()
    {
        string[] args = ["sections", "update", "smtp", "--body", """{"host":"smtp.example"}""", "--output", "none"];
        var (app, handler, _) = await CreateAppAsync(nameof(SectionCommand_OtherValuesInInlineBody_UpdatesTheSection), args, SectionDocument, SectionSchema);

        Assert.Equal(0, await app.RunAsync(args, null, TestContext.Current.CancellationToken));

        Assert.Equal(["GET /api/sections/smtp/schema", "PUT /api/sections/smtp"], handler.Requests);
    }

    [Fact]
    public async Task SectionCommand_WriteOnlyValueFromFile_IsSentWithoutReadingTheSchema()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var scratch = TestPaths.CreateScratchDirectory(nameof(SectionCommand_WriteOnlyValueFromFile_IsSentWithoutReadingTheSchema));
        var bodyFile = Path.Combine(scratch, "smtp.json");
        await File.WriteAllTextAsync(bodyFile, """{"password":"from-file"}""", cancellationToken);
        string[] args = ["sections", "update", "smtp", "--body-file", bodyFile, "--output", "none"];
        var (app, handler, _) = await CreateAppAsync(nameof(SectionCommand_WriteOnlyValueFromFile_IsSentWithoutReadingTheSchema), args, SectionDocument, SectionSchema);

        Assert.Equal(0, await app.RunAsync(args, null, cancellationToken));

        Assert.Equal(["PUT /api/sections/smtp"], handler.Requests);
        Assert.Contains("from-file", handler.Body);
    }

    [Fact]
    public async Task SectionCommand_SchemaUnavailable_DoesNotUpdate()
    {
        string[] args = ["sections", "update", "smtp", "--body", """{"password":"never-send-this"}""", "--output", "json"];
        var (app, handler, _) = await CreateAppAsync(nameof(SectionCommand_SchemaUnavailable_DoesNotUpdate), args, SectionDocument, schema: null);
        using var errors = new StringWriter();

        Assert.NotEqual(0, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));

        Assert.Equal(["GET /api/sections/smtp/schema"], handler.Requests);
    }

    [Fact]
    public async Task SectionCommand_WriteOnlyValueWithoutName_RefusesAnyInlineBody()
    {
        string[] args = ["sections", "update", "smtp", "--body", """{"host":"smtp.example"}""", "--output", "json"];
        var (app, handler, _) = await CreateAppAsync(nameof(SectionCommand_WriteOnlyValueWithoutName_RefusesAnyInlineBody), args, SectionDocument,
            """{"type":"object","additionalProperties":{"type":"string","writeOnly":true}}""");
        using var errors = new StringWriter();

        Assert.Equal(1, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));

        Assert.Equal(["GET /api/sections/smtp/schema"], handler.Requests);
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

    private const string SectionDocument = """
        {"paths":{"/api/sections/{name}":{"put":{"x-oc-cli":{"commandGroup":["sections"],"verb":"update","inputMode":"json",
            "requestSchemaPath":"api/sections/{name}/schema","arguments":[{"parameterName":"name","position":0}]},
          "parameters":[{"name":"name","in":"path","required":true,"schema":{"type":"string"}}],
          "requestBody":{"content":{"application/json":{"schema":{"type":"object"}}}}}}}}
        """;

    private const string SectionSchema = """
        {"type":"object","properties":{"host":{"type":"string","writeOnly":false},"password":{"$ref":"#/$defs/Secret"}},"$defs":{"Secret":{"type":"string","writeOnly":true}}}
        """;

    private static async Task<(CliApplication App, CapturingHandler Handler, CliPaths Paths)> CreateAppAsync(string name, string[] args, string document = Document, string? schema = "{}")
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
            Content = document,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, cancellationToken);
        var handler = new CapturingHandler(schema);
        var client = new HttpClient(handler);
        return (await CliApplication.CreateAsync(args, paths, client, cancellationToken, new UnsupportedCredentialStore()), handler, paths);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        private readonly string? _schema;

        public CapturingHandler(string? schema)
        {
            _schema = schema;
        }

        public string? Body { get; private set; }

        public List<string> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add($"{request.Method} {request.RequestUri!.AbsolutePath}");
            if (request.Method == HttpMethod.Get)
            {
                return _schema is null
                    ? new HttpResponseMessage(HttpStatusCode.Forbidden)
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(_schema) };
            }

            Body = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }
    }
}
