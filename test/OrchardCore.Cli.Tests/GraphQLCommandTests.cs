using System.Net;
using System.Text.Json.Nodes;

namespace OrchardCore.Cli.Tests;

public class GraphQLCommandTests
{
    [Theory]
    [InlineData(null, false, null)]
    [InlineData("{ __typename }", true, null)]
    [InlineData("{ __typename }", false, "Saved")]
    public async Task Build_ConflictingOrMissingDocument_RejectsInput(string? query, bool stdin, string? named)
    {
        await Assert.ThrowsAsync<CliException>(() => GraphQLRequestBuilder.CreateAsync(query, null, stdin, named,
            null, null, null, new StringReader("{ __typename }"), TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("null")]
    [InlineData("42")]
    public async Task Build_NonObjectVariables_RejectsInput(string variables)
    {
        await Assert.ThrowsAsync<CliException>(() => GraphQLRequestBuilder.CreateAsync("{ __typename }", null, false, null,
            variables, null, null, TextReader.Null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Build_Files_PreservesDocumentVariablesAndOperationName()
    {
        var directory = TestPaths.CreateScratchDirectory(nameof(Build_Files_PreservesDocumentVariablesAndOperationName));
        var queryFile = new FileInfo(Path.Combine(directory, "query.graphql"));
        var variablesFile = new FileInfo(Path.Combine(directory, "variables.json"));
        const string query = "query Read($name: String!) { __type(name: $name) { name } }";
        await File.WriteAllTextAsync(queryFile.FullName, query, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(variablesFile.FullName, """{"name":"Français","count":2,"enabled":true}""", TestContext.Current.CancellationToken);
        var request = await GraphQLRequestBuilder.CreateAsync(null, queryFile, false, null, null, variablesFile, "Read", TextReader.Null, TestContext.Current.CancellationToken);
        Assert.Equal(query, (string?)request["query"]);
        Assert.Equal("Read", (string?)request["operationName"]);
        Assert.Equal("Français", (string?)request["variables"]?["name"]);
        Assert.Equal(2, (int?)request["variables"]?["count"]);
        Assert.True((bool?)request["variables"]?["enabled"]);
    }

    [Fact]
    public async Task Build_StdinAndNamedQuery_UseTheExpectedTransportFields()
    {
        var input = await GraphQLRequestBuilder.CreateAsync(null, null, true, null, null, null, null,
            new StringReader("{ __typename }"), TestContext.Current.CancellationToken);
        Assert.Equal("{ __typename }", (string?)input["query"]);
        var named = await GraphQLRequestBuilder.CreateAsync(null, null, false, "RecentPosts", "{\"first\":3}", null, null,
            TextReader.Null, TestContext.Current.CancellationToken);
        Assert.Equal("RecentPosts", (string?)named["namedQuery"]);
        Assert.False(named.ContainsKey("query"));
    }

    [Theory]
    [InlineData(200, "{\"data\":{\"__typename\":\"Query\"}}", 0)]
    [InlineData(200, "{\"data\":null,\"errors\":[{\"message\":\"Denied\"}]}", 4)]
    [InlineData(400, "{\"data\":{\"name\":\"Partial\"},\"errors\":[{\"message\":\"Failure\"}]}", 4)]
    [InlineData(401, "{\"errors\":[{\"message\":\"Denied\"}]}", 4)]
    [InlineData(200, "{}", 1)]
    [InlineData(200, "<html>Login</html>", 3)]
    [InlineData(404, "<html>Not found</html>", 4)]
    public async Task Execute_DirectTransport_ReturnsExpectedExitCodeWithoutDiscoveryOrRetries(int status, string payload, int exitCode)
    {
        var handler = new Handler((request, body) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal("https://cms.example/blog/api/graphql", request.RequestUri!.AbsoluteUri);
            Assert.Equal("application/json", request.Content!.Headers.ContentType!.MediaType);
            Assert.Contains(request.Headers.Accept, value => value.MediaType == "application/graphql-response+json");
            Assert.Equal("{ __typename }", (string?)JsonNode.Parse(body)?["query"]);
            return new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(payload) };
        });
        var args = new[] { "graphql", "execute", "--query", "{ __typename }", "--output", "none" };
        var paths = await CreatePathsAsync();
        using var http = new HttpClient(handler);
        var app = await CliApplication.CreateAsync(args, paths, http, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        Assert.Equal(exitCode, await app.InvokeAsync(args));
        Assert.Equal(1, handler.Count);
    }

    [Theory]
    [InlineData("https://evil.example/graphql")]
    [InlineData("../other/api/graphql")]
    [InlineData("api/%2fgraphql")]
    public async Task Execute_UnsafeEndpoint_RejectsBeforeSendingRequest(string endpoint)
    {
        var handler = new Handler((_, _) => throw new InvalidOperationException("Unexpected network access."));
        var args = new[] { "graphql", "execute", "--query", "{ __typename }", "--endpoint", endpoint };
        using var http = new HttpClient(handler);
        var app = await CliApplication.CreateAsync(args, await CreatePathsAsync(), http, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        Assert.Equal(1, await app.InvokeAsync(args));
        Assert.Equal(0, handler.Count);
    }

    [Fact]
    public async Task Schema_CustomEndpoint_UsesIntrospectionAndVariablesWithoutOpenApi()
    {
        var handler = new Handler((request, body) =>
        {
            Assert.Equal("https://cms.example/blog/custom/graphql", request.RequestUri!.AbsoluteUri);
            var document = JsonNode.Parse(body)!;
            Assert.Contains("__type(name: $name)", (string?)document["query"]);
            Assert.Equal("Article", (string?)document["variables"]?["name"]);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"data\":{\"__type\":{\"name\":\"Article\"}}}") };
        });
        var args = new[] { "graphql", "schema", "--type", "Article", "--endpoint", "custom/graphql", "--output", "none" };
        using var http = new HttpClient(handler);
        var app = await CliApplication.CreateAsync(args, await CreatePathsAsync(), http, TestContext.Current.CancellationToken, new UnsupportedCredentialStore());
        Assert.Equal(0, await app.InvokeAsync(args));
        Assert.Equal(1, handler.Count);
    }

    private static async Task<CliPaths> CreatePathsAsync()
    {
        var paths = new CliPaths(TestPaths.CreateScratchDirectory(nameof(GraphQLCommandTests)));
        await new ContextStore(paths).SaveAsync(new CliConfiguration
        {
            CurrentContext = "blog",
            Contexts = [new TenantContextRecord { Name = "blog", TenantUrl = "https://cms.example/blog/" }],
        }, TestContext.Current.CancellationToken);
        return paths;
    }

    private sealed class Handler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, string, HttpResponseMessage> _respond;

        public Handler(Func<HttpRequestMessage, string, HttpResponseMessage> respond) => _respond = respond;

        public int Count { get; private set; }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Count++;
            return _respond(request, request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
        }
    }
}
