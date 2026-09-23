using System.Net;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Http.Models;
using OrchardCore.Workflows.Http.Services;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Http;

public class HttpRequestTaskTests
{
    private static readonly IDictionary<string, object> s_emptyDictionary = new Dictionary<string, object>();

    [Fact]
    public async Task ExecuteAsync_RequestDrivenMetadataUrl_IsBlockedBeforeConnecting()
    {
        var validator = new HttpRequestDestinationValidator(Options.Create(new HttpRequestTaskOptions()));
        using var handler = HttpRequestTaskHttpClient.CreateHandler(validator);
        using var client = new HttpClient(handler);
        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(factory => factory.CreateClient(HttpRequestTaskHttpClient.Name))
            .Returns(client);
        var task = CreateTask(
            httpClientFactory.Object,
            expression => expression.Contains("Request.UriHost", StringComparison.Ordinal)
                ? "http://169.254.169.254/latest/meta-data/"
                : expression);
        task.Url = new WorkflowExpression<string>("http://{{ Request.UriHost }}/latest/meta-data/");

        using var executionContext = CreateExecutionContext();
        var result = await task.ExecuteAsync(executionContext, Mock.Of<ActivityContext>());

        Assert.Equal("UnhandledHttpStatus", result.Outcomes.Single());
        httpClientFactory.Verify(factory => factory.CreateClient(HttpRequestTaskHttpClient.Name), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_HostHeaderOverride_DoesNotCreateHttpClient()
    {
        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        var task = CreateTask(httpClientFactory.Object);
        task.Url = new WorkflowExpression<string>("https://example.com/");
        task.Headers = new WorkflowExpression<string>("Host: internal.example.com");

        using var executionContext = CreateExecutionContext();
        var result = await task.ExecuteAsync(executionContext, Mock.Of<ActivityContext>());

        Assert.Equal("UnhandledHttpStatus", result.Outcomes.Single());
        httpClientFactory.Verify(factory => factory.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ValidPublicUrl_UsesProtectedNamedClient()
    {
        var messageHandler = new StubHttpMessageHandler();
        using var client = new HttpClient(messageHandler);
        var httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        httpClientFactory
            .Setup(factory => factory.CreateClient(HttpRequestTaskHttpClient.Name))
            .Returns(client);
        var task = CreateTask(httpClientFactory.Object);
        task.Url = new WorkflowExpression<string>("https://example.com/webhook");

        using var executionContext = CreateExecutionContext();
        var result = await task.ExecuteAsync(executionContext, Mock.Of<ActivityContext>());

        Assert.Equal("200", result.Outcomes.Single());
        Assert.Equal(new Uri("https://example.com/webhook"), messageHandler.RequestUri);
        httpClientFactory.Verify(factory => factory.CreateClient(HttpRequestTaskHttpClient.Name), Times.Once);
    }

    private static HttpRequestTask CreateTask(IHttpClientFactory httpClientFactory, Func<string, string> evaluate = null)
    {
        return new HttpRequestTask(
            new StubWorkflowExpressionEvaluator(evaluate ?? (expression => expression)),
            UrlEncoder.Default,
            httpClientFactory,
            Mock.Of<IStringLocalizer<HttpRequestTask>>(),
            Mock.Of<ILogger<HttpRequestTask>>());
    }

    private static WorkflowExecutionContext CreateExecutionContext()
    {
        return new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow(),
            s_emptyDictionary,
            s_emptyDictionary,
            s_emptyDictionary,
            [],
            default,
            []);
    }

    private sealed class StubWorkflowExpressionEvaluator : IWorkflowExpressionEvaluator
    {
        private readonly Func<string, string> _evaluate;

        public StubWorkflowExpressionEvaluator(Func<string, string> evaluate)
        {
            _evaluate = evaluate;
        }

        public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder)
            => Task.FromResult((T)(object)_evaluate(expression.Expression ?? string.Empty));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        public Uri RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("OK"),
            });
        }
    }
}
