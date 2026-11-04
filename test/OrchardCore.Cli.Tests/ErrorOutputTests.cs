using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace OrchardCore.Cli.Tests;

public class ErrorOutputTests
{
    [Theory]
    [InlineData("human")]
    [InlineData("json")]
    [InlineData("auto")]
    [InlineData("none")]
    public async Task Invoke_ApiProblem_UsesSelectedErrorFormatAndPreservesExitCode(string format)
    {
        const string detail = "The feature 'OrchardCore.ArchiveLater' could not be enabled. Check its dependencies and the active feature profile.";
        var payload = new JsonObject { ["title"] = "Bad request", ["detail"] = detail, ["status"] = 400 };
        using var http = new HttpClient(new ResponseHandler(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/problem+json"),
            };
            response.Headers.Add("X-Correlation-ID", "test-request-id");
            return response;
        }));
        var args = new[] { "api", "invoke", "POST", "api/features/OrchardCore.ArchiveLater:enable", "--output", format };
        using var errors = new StringWriter();
        var app = await CliApplication.CreateAsync(args, await CreatePathsAsync(), http, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Equal(4, await app.InvokeAsync(args, errors));
        var output = errors.ToString();
        if (CliUtilities.ParseOutputFormat(format) == OutputFormat.Human)
        {
            Assert.StartsWith("An error occurred.", output);
            Assert.Contains("POST /blog/api/features/OrchardCore.ArchiveLater:enable failed with 400", output);
            Assert.Contains(detail, output);
            Assert.Contains("Request ID: test-request-id", output);
            Assert.DoesNotContain("\"error\"", output);
            Assert.DoesNotContain("\\u0027", output);
        }
        else
        {
            var error = JsonNode.Parse(output)!["error"]!;
            Assert.Equal("api_error", (string?)error["code"]);
            Assert.Equal(400, (int?)error["status"]);
            Assert.Equal(detail, (string?)error["details"]?["detail"]);
            Assert.Equal("test-request-id", (string?)error["correlationId"]);
        }
    }

    [Theory]
    [InlineData("human", false)]
    [InlineData("json", false)]
    [InlineData("human", true)]
    [InlineData("json", true)]
    public async Task Invoke_TransportOrJsonFailure_UsesSelectedErrorFormat(string format, bool invalidJson)
    {
        using var http = new HttpClient(new ResponseHandler(() => invalidJson
            ? new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("not JSON", Encoding.UTF8, "application/json") }
            : throw new HttpRequestException("Connection refused (localhost:5001)")));
        var args = new[] { "api", "invoke", "GET", "api/test", "--output", format };
        using var errors = new StringWriter();
        var app = await CliApplication.CreateAsync(args, await CreatePathsAsync(), http, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());

        Assert.Equal(invalidJson ? 3 : 2, await app.InvokeAsync(args, errors));
        if (format == "human")
        {
            Assert.StartsWith("An error occurred.", errors.ToString());
            Assert.DoesNotContain("\"error\"", errors.ToString());
        }
        else
        {
            Assert.Equal(invalidJson ? "invalid_json" : "http_error", (string?)JsonNode.Parse(errors.ToString())?["error"]?["code"]);
        }
    }

    [Theory]
    [InlineData("{\"title\":\"Validation failed\",\"errors\":{\"Email\":[\"Enter a valid email.\",\"Email is required.\"]}}", "Email: Enter a valid email.\nEmail: Email is required.")]
    [InlineData("{\"error\":\"invalid_scope\",\"error_description\":\"This scope is not allowed.\"}", "This scope is not allowed.")]
    [InlineData("{\"error\":{\"message\":\"Permission denied.\"}}", "Permission denied.")]
    [InlineData("{\"errors\":[{\"message\":\"GraphQL field denied.\"}]}", "GraphQL field denied.")]
    [InlineData("{\"title\":\"Not found\"}", "Not found")]
    [InlineData("{\"detail\":42,\"message\":\"Unusual response.\"}", "Unusual response.")]
    public void Format_ServerErrorShapes_ShowsReadableMessages(string payload, string expected)
    {
        var output = HumanErrorFormatter.Format("Request failed.", JsonNode.Parse(payload));
        Assert.Contains(expected.Replace("\n", global::System.Environment.NewLine), output);
        Assert.DoesNotContain("{", output);
    }

    [Fact]
    public void Format_UnrecognizedDetails_DoesNotDumpMetadataOrTerminalControls()
    {
        var output = HumanErrorFormatter.Format("Request failed.\u001b\r", JsonNode.Parse("{\"stackTrace\":\"internal implementation\",\"status\":500}"), "request\u001bid");
        Assert.Contains("Request failed.", output);
        Assert.DoesNotContain("internal implementation", output);
        Assert.DoesNotContain('\u001b', output);
        // Formatter-owned line endings are CRLF on Windows. Assert the exact
        // sanitized lines so injected controls are still rejected on every OS.
        Assert.Equal(string.Join(global::System.Environment.NewLine,
            "An error occurred.", "Request failed.", "Request ID: requestid"), output);
    }

    private static async Task<CliPaths> CreatePathsAsync()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(ErrorOutputTests)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "blog",
            Contexts = [new TenantContextRecord { Name = "blog", TenantUrl = "https://cms.example/blog/" }],
        }, TestContext.Current.CancellationToken);
        return paths;
    }

    private sealed class ResponseHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _respond;

        public ResponseHandler(Func<HttpResponseMessage> respond)
        {
            _respond = respond;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_respond());
    }
}
