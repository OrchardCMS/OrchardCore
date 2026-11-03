using System.Net;
using System.Text.Json.Nodes;

namespace OrchardCore.Cli.Tests;

public class RequestTimeoutTests
{
    private static readonly TimeSpan _shortTimeout = TimeSpan.FromMilliseconds(100);
    private static readonly TimeSpan _longerThanTimeout = TimeSpan.FromMilliseconds(500);

    [Fact]
    public void CreateClient_DefaultClient_HasNoClientWideTimeout()
    {
        using var client = CliHttp.CreateClient();

        Assert.Equal(Timeout.InfiniteTimeSpan, client.Timeout);
    }

    [Theory]
    [InlineData("human")]
    [InlineData("json")]
    public async Task Invoke_RequestExceedsTimeout_ReportsGenericFailureAndMessage(string format)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = new HttpClient(new DelayingHandler(Timeout.InfiniteTimeSpan));
        string[] args = ["api", "invoke", "GET", "api/test", "--output", format];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(Invoke_RequestExceedsTimeout_ReportsGenericFailureAndMessage), client);

        Assert.Equal(2, await app.RunAsync(args, errors, cancellationToken));

        if (format == "json")
        {
            var error = JsonNode.Parse(errors.ToString())!["error"]!;
            Assert.Equal("timeout", (string?)error["code"]);
            Assert.Contains("timed out", (string?)error["message"]);
        }
        else
        {
            Assert.Contains("timed out", errors.ToString());
        }
    }

    [Fact]
    public async Task Invoke_HttpClientTimeoutExpires_ReportsGenericFailure()
    {
        using var client = new HttpClient(new DelayingHandler(Timeout.InfiniteTimeSpan)) { Timeout = _shortTimeout };
        string[] args = ["api", "invoke", "GET", "api/test", "--output", "json"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(Invoke_HttpClientTimeoutExpires_ReportsGenericFailure), client, Timeout.InfiniteTimeSpan);

        Assert.Equal(2, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("api", "invoke", "GET", "api/test")]
    [InlineData("graphql", "execute", "--query", "{ test }")]
    public async Task Invoke_CallerCancels_DoesNotReportTimeout(params string[] command)
    {
        using var client = new HttpClient(new DelayingHandler(Timeout.InfiniteTimeSpan));
        string[] args = [.. command, "--output", "json"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(Invoke_CallerCancels_DoesNotReportTimeout), client, TimeSpan.FromMinutes(1));
        using var source = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        source.CancelAfter(_shortTimeout);

        var exception = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => app.RunAsync(args, errors, source.Token));

        Assert.IsNotType<TimeoutException>(exception.InnerException);
        Assert.Empty(errors.ToString());
    }

    [Fact]
    public async Task SendWithTimeoutAsync_TimeLimitExpires_ReportsTimeout()
    {
        using var client = new HttpClient(new DelayingHandler(Timeout.InfiniteTimeSpan));
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://cms.example/");

        var exception = await Assert.ThrowsAsync<TaskCanceledException>(() => client.SendWithTimeoutAsync(request, _shortTimeout, TestContext.Current.CancellationToken));

        Assert.True(CliHttp.IsTimeout(exception));
    }

    [Fact]
    public async Task Upload_SlowerThanRequestTimeout_Completes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var scratch = TestPaths.CreateScratchDirectory(nameof(Upload_SlowerThanRequestTimeout_Completes));
        var file = Path.Combine(scratch, "package.bin");
        await File.WriteAllBytesAsync(file, [1, 2, 3], cancellationToken);
        var handler = new DelayingHandler(_longerThanTimeout);
        using var client = new HttpClient(handler);
        string[] args = ["files", "upload", "--file", file, "--output", "none"];
        var app = await CreateAppAsync(args, nameof(Upload_SlowerThanRequestTimeout_Completes), client);

        Assert.Equal(0, await app.RunAsync(args, null, cancellationToken));
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task JsonRequest_SlowerThanRequestTimeout_TimesOut()
    {
        using var client = new HttpClient(new DelayingHandler(_longerThanTimeout));
        string[] args = ["files", "update", "--body", "{}", "--output", "none"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(JsonRequest_SlowerThanRequestTimeout_TimesOut), client);

        Assert.Equal(2, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task LongRunningOperation_SlowerThanRequestTimeout_Completes()
    {
        var handler = new DelayingHandler(_longerThanTimeout);
        using var client = new HttpClient(handler);
        string[] args = ["files", "install", "--body", "{}", "--output", "none"];
        var app = await CreateAppAsync(args, nameof(LongRunningOperation_SlowerThanRequestTimeout_Completes), client);

        Assert.Equal(0, await app.RunAsync(args, null, TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task LongRunningOperation_ResponseBodySlowerThanRequestTimeout_Completes()
    {
        using var client = new HttpClient(new DelayingHandler(TimeSpan.Zero, new SlowStream(_longerThanTimeout)));
        string[] args = ["files", "install", "--body", "{}", "--output", "none"];
        var app = await CreateAppAsync(args, nameof(LongRunningOperation_ResponseBodySlowerThanRequestTimeout_Completes), client);

        Assert.Equal(0, await app.RunAsync(args, null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RequestTimeoutOption_LongerThanDelay_Completes()
    {
        var handler = new DelayingHandler(_longerThanTimeout);
        using var client = new HttpClient(handler);
        string[] args = ["files", "update", "--body", "{}", "--request-timeout", "60", "--output", "none"];
        var app = await CreateAppAsync(args, nameof(RequestTimeoutOption_LongerThanDelay_Completes), client);

        Assert.Equal(0, await app.RunAsync(args, null, TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.Requests);
    }

    [Theory]
    [InlineData("files", "install", "--body", "{}")]
    [InlineData("api", "invoke", "POST", "api/test")]
    public async Task RequestTimeoutOption_ShorterThanDelay_TimesOut(params string[] command)
    {
        using var client = new HttpClient(new DelayingHandler(Timeout.InfiniteTimeSpan));
        string[] args = [.. command, "--request-timeout", "1", "--output", "json"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(RequestTimeoutOption_ShorterThanDelay_TimesOut), client);

        Assert.Equal(2, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));
        Assert.Equal("timeout", (string?)JsonNode.Parse(errors.ToString())!["error"]!["code"]);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("86401")]
    public async Task RequestTimeoutOption_OutOfRange_IsRejectedBeforeAnyRequest(string seconds)
    {
        var handler = new DelayingHandler(TimeSpan.Zero);
        using var client = new HttpClient(handler);
        string[] args = ["files", "update", "--body", "{}", "--request-timeout=" + seconds, "--output", "json"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(RequestTimeoutOption_OutOfRange_IsRejectedBeforeAnyRequest), client);

        Assert.NotEqual(0, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));
        Assert.Contains("--request-timeout", errors.ToString());
        Assert.Equal(0, handler.Requests);
    }

    [Fact]
    public async Task RequestTimeoutOption_BinaryUpload_IsNotOffered()
    {
        using var client = new HttpClient(new DelayingHandler(TimeSpan.Zero));
        string[] args = ["files", "upload", "--help"];
        var app = await CreateAppAsync(args, nameof(RequestTimeoutOption_BinaryUpload_IsNotOffered), client);

        var files = Assert.Single(app.RootCommand.Subcommands, command => command.Name == "files");
        Assert.DoesNotContain(Assert.Single(files.Subcommands, command => command.Name == "upload").Options, option => option.Name == "--request-timeout");
        Assert.Contains(Assert.Single(files.Subcommands, command => command.Name == "update").Options, option => option.Name == "--request-timeout");
    }

    [Fact]
    public async Task EnvironmentVariable_LongerThanDelay_CompletesOrdinaryRequest()
    {
        var handler = new DelayingHandler(_longerThanTimeout);
        using var client = new HttpClient(handler);
        string[] args = ["files", "update", "--body", "{}", "--output", "none"];
        var app = await CreateAppAsync(args, nameof(EnvironmentVariable_LongerThanDelay_CompletesOrdinaryRequest), client, variable: "60");

        Assert.Equal(0, await app.RunAsync(args, null, TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task EnvironmentVariable_ShorterThanDelay_LimitsLongRunningOperation()
    {
        using var client = new HttpClient(new DelayingHandler(Timeout.InfiniteTimeSpan));
        string[] args = ["files", "install", "--body", "{}", "--output", "json"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(EnvironmentVariable_ShorterThanDelay_LimitsLongRunningOperation), client, variable: "1");

        Assert.Equal(2, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task RequestTimeoutOption_WithEnvironmentVariable_Wins()
    {
        var handler = new DelayingHandler(_longerThanTimeout);
        using var client = new HttpClient(handler);
        string[] args = ["files", "update", "--body", "{}", "--request-timeout", "60", "--output", "none"];
        var app = await CreateAppAsync(args, nameof(RequestTimeoutOption_WithEnvironmentVariable_Wins), client, variable: "86400");

        Assert.Equal(0, await app.RunAsync(args, null, TestContext.Current.CancellationToken));
        Assert.Equal(1, handler.Requests);
    }

    [Theory]
    [InlineData("soon")]
    [InlineData("0")]
    [InlineData("86401")]
    public async Task EnvironmentVariable_Invalid_ReportsIt(string value)
    {
        using var client = new HttpClient(new DelayingHandler(TimeSpan.Zero));

        var exception = await Assert.ThrowsAsync<CliException>(() => CreateAppAsync(["context", "list"], nameof(EnvironmentVariable_Invalid_ReportsIt), client, variable: value));

        Assert.Contains("OC_REQUEST_TIMEOUT", exception.Message);
    }

    [Fact]
    public async Task Download_BodySlowerThanRequestTimeout_Completes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var scratch = TestPaths.CreateScratchDirectory(nameof(Download_BodySlowerThanRequestTimeout_Completes));
        var destination = Path.Combine(scratch, "package.bin");
        using var client = new HttpClient(new DelayingHandler(TimeSpan.Zero, new SlowStream(_longerThanTimeout)));
        string[] args = ["files", "download", "--output-file", destination, "--output", "none"];
        var app = await CreateAppAsync(args, nameof(Download_BodySlowerThanRequestTimeout_Completes), client);

        Assert.Equal(0, await app.RunAsync(args, null, cancellationToken));
        Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(destination, cancellationToken));
    }

    [Fact]
    public async Task JsonResponse_BodySlowerThanRequestTimeout_TimesOut()
    {
        using var client = new HttpClient(new DelayingHandler(TimeSpan.Zero, new SlowStream(_longerThanTimeout)));
        string[] args = ["api", "invoke", "GET", "api/test", "--output", "none"];
        using var errors = new StringWriter();
        var app = await CreateAppAsync(args, nameof(JsonResponse_BodySlowerThanRequestTimeout_TimesOut), client);

        Assert.Equal(2, await app.RunAsync(args, errors, TestContext.Current.CancellationToken));
    }

    private static async Task<CliApplication> CreateAppAsync(string[] args, string name, HttpClient client, TimeSpan? requestTimeout = null, string? variable = null)
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
            Content = """
            {"paths":{
              "/api/upload":{"post":{"operationId":"Upload","x-oc-cli":{"commandGroup":["files"],"verb":"upload"},
                "requestBody":{"content":{"application/octet-stream":{"schema":{"type":"string","format":"binary"}}}}}},
              "/api/update":{"post":{"operationId":"Update","x-oc-cli":{"commandGroup":["files"],"verb":"update","inputMode":"json"},
                "requestBody":{"content":{"application/json":{"schema":{"type":"object"}}}}}},
              "/api/install":{"post":{"operationId":"Install","x-oc-cli":{"commandGroup":["files"],"verb":"install","inputMode":"json","longRunning":true},
                "requestBody":{"content":{"application/json":{"schema":{"type":"object"}}}}}},
              "/api/download":{"get":{"operationId":"Download","x-oc-cli":{"commandGroup":["files"],"verb":"download","fileResponse":true}}}
            }}
            """,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
        }, cancellationToken);

        return await CliApplication.CreateAsync(args, paths, client, cancellationToken, new UnsupportedCredentialStore(), key => key == "OC_REQUEST_TIMEOUT" ? variable : null, requestTimeout ?? _shortTimeout);
    }

    private sealed class DelayingHandler : HttpMessageHandler
    {
        private readonly TimeSpan _delay;
        private readonly Stream? _body;

        public DelayingHandler(TimeSpan delay, Stream? body = null)
        {
            _delay = delay;
            _body = body;
        }

        public int Requests { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);
            Requests++;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = _body is null ? new StringContent("{}") : new StreamContent(_body),
            };
        }
    }

    private sealed class SlowStream : MemoryStream
    {
        private readonly TimeSpan _delay;

        public SlowStream(TimeSpan delay)
            : base([1, 2, 3])
        {
            _delay = delay;
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await Task.Delay(_delay, cancellationToken);
            return await base.ReadAsync(buffer, cancellationToken);
        }
    }
}
