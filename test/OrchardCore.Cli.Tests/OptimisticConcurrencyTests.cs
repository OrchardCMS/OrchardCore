using System.Net;

namespace OrchardCore.Cli.Tests;

public class OptimisticConcurrencyTests
{
    private const string Document = """
        {"paths":{"/api/widgets/{id}":{
          "get":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"show","arguments":[{"parameterName":"id","position":0}]},
            "parameters":[{"name":"id","in":"path","required":true,"schema":{"type":"string"}}],
            "responses":{"200":{"description":"OK","headers":{"ETag":{"schema":{"type":"string"}}}}}},
          "put":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"update","inputMode":"json","arguments":[{"parameterName":"id","position":0}]},
            "parameters":[{"name":"id","in":"path","required":true,"schema":{"type":"string"}},
              {"name":"If-Match","in":"header","schema":{"type":"string"}}],
            "requestBody":{"content":{"application/json":{"schema":{"type":"object","properties":{"name":{"type":"string"}}}}}},
            "responses":{"200":{"description":"OK","headers":{"ETag":{"schema":{"type":"string"}}}},"412":{"description":"Precondition Failed"}}},
          "delete":{"x-oc-cli":{"commandGroup":["widgets"],"verb":"delete","arguments":[{"parameterName":"id","position":0}]},
            "parameters":[{"name":"id","in":"path","required":true,"schema":{"type":"string"}}],
            "responses":{"204":{"description":"No Content"}}}}}}
        """;

    [Fact]
    public async Task DynamicCommand_OptionalIfMatchHeader_IsAnOption()
    {
        string[] args = ["widgets", "update", "--help"];
        var (app, _) = await CreateAppAsync(nameof(DynamicCommand_OptionalIfMatchHeader_IsAnOption), args);

        var update = Assert.Single(Assert.Single(app.RootCommand.Subcommands, command => command.Name == "widgets").Subcommands, command => command.Name == "update");
        var option = Assert.Single(update.Options, option => option.Name == "--if-match");
        Assert.False(option.Required);
    }

    [Fact]
    public async Task DynamicCommand_IfMatchOption_SendsTheHeader()
    {
        string[] args = ["widgets", "update", "a", "--if-match", "\"abc\"", "--body", """{"name":"x"}""", "--output", "none"];
        var (app, handler) = await CreateAppAsync(nameof(DynamicCommand_IfMatchOption_SendsTheHeader), args);

        Assert.Equal(0, await app.InvokeAsync(args));

        Assert.Equal("\"abc\"", handler.IfMatch);
    }

    [Fact]
    public async Task DynamicCommand_NoIfMatchOption_SendsNoHeader()
    {
        string[] args = ["widgets", "update", "a", "--body", """{"name":"x"}""", "--output", "none"];
        var (app, handler) = await CreateAppAsync(nameof(DynamicCommand_NoIfMatchOption_SendsNoHeader), args);

        Assert.Equal(0, await app.InvokeAsync(args));

        Assert.True(handler.Called);
        Assert.Null(handler.IfMatch);
    }

    [Fact]
    public async Task DynamicCommand_PreconditionFailed_ReportsTheCodeOfTheProblem()
    {
        string[] args = ["widgets", "update", "a", "--if-match", "\"stale\"", "--body", """{"name":"x"}""", "--output", "json"];
        var (app, handler) = await CreateAppAsync(nameof(DynamicCommand_PreconditionFailed_ReportsTheCodeOfTheProblem), args);
        handler.Failure = """{"title":"Precondition failed","status":412,"code":"precondition_failed"}""";
        using var output = new StringWriter();

        Assert.Equal(4, await app.InvokeAsync(args, output));

        Assert.Contains("\"code\":\"precondition_failed\"", output.ToString());
        Assert.Contains("\"status\":412", output.ToString());
    }

    [Theory]
    [InlineData("show", true)]
    [InlineData("update", true)]
    [InlineData("delete", false)]
    public async Task DynamicCommand_DeclaredETagHeader_AddsTheShowETagOption(string verb, bool expected)
    {
        string[] args = ["widgets", verb, "--help"];
        var (app, _) = await CreateAppAsync(nameof(DynamicCommand_DeclaredETagHeader_AddsTheShowETagOption) + verb, args);

        var command = Assert.Single(Assert.Single(app.RootCommand.Subcommands, command => command.Name == "widgets").Subcommands, command => command.Name == verb);
        Assert.Equal(expected, command.Options.Any(option => option.Name == "--show-etag"));
    }

    private static async Task<(CliApplication App, CapturingHandler Handler)> CreateAppAsync(string name, string[] args)
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
        return (await CliApplication.CreateAsync(args, paths, new HttpClient(handler), cancellationToken, new UnsupportedCredentialStore()), handler);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public bool Called { get; private set; }

        public string? IfMatch { get; private set; }

        public string? Failure { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Called = true;
            IfMatch = request.Headers.TryGetValues("If-Match", out var values) ? values.Single() : null;
            return Task.FromResult(Failure is null
                ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") }
                : new HttpResponseMessage(HttpStatusCode.PreconditionFailed) { Content = new StringContent(Failure) });
        }
    }
}
