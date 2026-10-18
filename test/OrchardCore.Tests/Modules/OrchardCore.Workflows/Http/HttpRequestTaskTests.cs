using System.Net;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Http.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Http;

public class HttpRequestTaskTests
{
    [Theory]
    [InlineData("@evil.com")]
    [InlineData("x@evil.com/")]
    [InlineData("../../admin")]
    [InlineData("a?redirect=https://evil.com")]
    [InlineData("a#fragment")]
    [InlineData("a b/c")]
    public async Task ExecuteAsync_UserValueInUrl_IsUrlEncoded(string userValue)
    {
        // Arrange
        var handler = new CapturingHttpMessageHandler();
        var task = CreateTask(handler);
        task.Url = new WorkflowExpression<string>("https://example.com/items/{{ Workflow.Input.Value }}");

        using var workflowContext = CreateWorkflowContext(userValue);

        // Act
        await task.ExecuteAsync(workflowContext, Mock.Of<ActivityContext>());

        // Assert
        var requestUri = Assert.Single(handler.Requests).RequestUri;
        Assert.Equal("example.com", requestUri.Host);
        Assert.Equal(string.Empty, requestUri.UserInfo);
        Assert.Equal(string.Empty, requestUri.Query);
        Assert.Equal(string.Empty, requestUri.Fragment);
        Assert.Equal("/items/" + UrlEncoder.Default.Encode(userValue), requestUri.AbsolutePath);
    }

    [Fact]
    public async Task ExecuteAsync_UserValueInUrlWithRawFilter_IsNotEncoded()
    {
        // Arrange
        var handler = new CapturingHttpMessageHandler();
        var task = CreateTask(handler);
        task.Url = new WorkflowExpression<string>("https://example.com/items/{{ Workflow.Input.Value | raw }}");

        using var workflowContext = CreateWorkflowContext("a/b?c=d");

        // Act
        await task.ExecuteAsync(workflowContext, Mock.Of<ActivityContext>());

        // Assert
        var requestUri = Assert.Single(handler.Requests).RequestUri;
        Assert.Equal("/items/a/b", requestUri.AbsolutePath);
        Assert.Equal("?c=d", requestUri.Query);
    }

    [Fact]
    public async Task ExecuteAsync_UserValueInHeader_CannotInjectHeaders()
    {
        // Arrange
        var handler = new CapturingHttpMessageHandler();
        var task = CreateTask(handler);
        task.Url = new WorkflowExpression<string>("https://example.com/");
        task.Headers = new WorkflowExpression<string>("X-Value: {{ Workflow.Input.Value }}");

        using var workflowContext = CreateWorkflowContext("abc\r\nX-Injected: 1");

        // Act
        await task.ExecuteAsync(workflowContext, Mock.Of<ActivityContext>());

        // Assert
        var request = Assert.Single(handler.Requests);
        Assert.False(request.Headers.Contains("X-Injected"));
        Assert.Equal(UrlEncoder.Default.Encode("abc\r\nX-Injected: 1"), Assert.Single(request.Headers.GetValues("X-Value")).Trim());
    }

    private static HttpRequestTask CreateTask(HttpMessageHandler handler)
    {
        var services = new ServiceCollection();
        services.Configure<FluidParserOptions>(_ => { });
        services.Configure<LiquidViewOptions>(_ => { });
        services.Configure<TemplateOptions>(options =>
        {
            options.MemberAccessStrategy.Register<LiquidPropertyAccessor, FluidValue>((obj, name) => obj.GetValueAsync(name));
            options.MemberAccessStrategy.Register<WorkflowExecutionContext>();
            options.MemberAccessStrategy.Register<WorkflowExecutionContext, LiquidPropertyAccessor>("Input", (obj, context) => new LiquidPropertyAccessor((LiquidTemplateContext)context, (name, context) => LiquidWorkflowExpressionEvaluator.ToFluidValue(obj.Input, name, context)));
        });
        services.AddHttpContextAccessor();
        services.AddScoped(_ => new ViewContextAccessor { ViewContext = new ViewContext() });
        services.AddScoped(_ => Mock.Of<IViewLocalizer>());
        var localClock = new Mock<ILocalClock>();
        localClock.Setup(clock => clock.GetLocalNowAsync()).ReturnsAsync(DateTimeOffset.UtcNow);
        localClock.Setup(clock => clock.GetLocalTimeZoneAsync()).ReturnsAsync(Mock.Of<ITimeZone>(timeZone => timeZone.TimeZoneId == "UTC"));
        services.AddScoped(_ => localClock.Object);

        var serviceProvider = services.BuildServiceProvider();

        var evaluator = new LiquidWorkflowExpressionEvaluator(
            new LiquidViewParser(
                serviceProvider.GetRequiredService<IOptions<LiquidViewOptions>>(),
                serviceProvider.GetRequiredService<IOptions<FluidParserOptions>>()),
            [],
            serviceProvider,
            Mock.Of<ILogger<LiquidWorkflowExpressionEvaluator>>(),
            serviceProvider.GetRequiredService<IOptions<TemplateOptions>>());

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(handler, disposeHandler: false));

        return new HttpRequestTask(
            evaluator,
            UrlEncoder.Default,
            httpClientFactory.Object,
            Mock.Of<IStringLocalizer<HttpRequestTask>>());
    }

    private static WorkflowExecutionContext CreateWorkflowContext(string value)
        => new(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            new Dictionary<string, object> { ["Value"] = value },
            new Dictionary<string, object>(),
            new Dictionary<string, object>(),
            [],
            default,
            []);

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(string.Empty),
            });
        }
    }
}
