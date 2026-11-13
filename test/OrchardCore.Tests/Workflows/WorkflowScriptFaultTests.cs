using Jint;
using Jint.Runtime;
using OrchardCore.Scripting;
using OrchardCore.Scripting.JavaScript;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Tests.Workflows;

public class WorkflowScriptFaultTests
{
    [Fact]
    public async Task EvaluateAsync_ScriptStoppedByTheStatementLimit_Throws()
    {
        var evaluator = CreateEvaluator(options => options.MaxStatements(1_000));
        using var workflowContext = CreateWorkflowContext();

        // The script never produced a value, so there is no value to carry on with: false here would send
        // the workflow down an outcome nobody decided on.
        await Assert.ThrowsAsync<StatementsCountOverflowException>(
            () => evaluator.EvaluateAsync(new WorkflowExpression<bool>("while (true) {} true"), workflowContext));
    }

    [Fact]
    public async Task EvaluateAsync_ScriptStoppedByTheTimeLimit_Throws()
    {
        var evaluator = CreateEvaluator(options => options.TimeoutInterval(TimeSpan.FromMilliseconds(100)));
        using var workflowContext = CreateWorkflowContext();

        await Assert.ThrowsAsync<TimeoutException>(
            () => evaluator.EvaluateAsync(new WorkflowExpression<bool>("while (true) {} true"), workflowContext));
    }

    [Fact]
    public async Task EvaluateAsync_CancelledEvaluation_Throws()
    {
        var evaluator = new JavaScriptWorkflowScriptEvaluator(
            new ThrowingScriptingManager(new OperationCanceledException()),
            [],
            new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object);

        using var workflowContext = CreateWorkflowContext();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => evaluator.EvaluateAsync(new WorkflowExpression<bool>("true"), workflowContext));
    }

    [Theory]
    [InlineData("throw new Error('boom')")]
    [InlineData("null")]
    [InlineData("this is not javascript")]
    public async Task EvaluateAsync_ScriptErrorOrUnconvertibleResult_StillReturnsTheDefaultValue(string script)
    {
        var evaluator = CreateEvaluator(options => options.MaxStatements(1_000));
        using var workflowContext = CreateWorkflowContext();

        Assert.False(await evaluator.EvaluateAsync(new WorkflowExpression<bool>(script), workflowContext));
    }

    private static JavaScriptWorkflowScriptEvaluator CreateEvaluator(Action<Jint.Options> configure)
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .AddScripting()
            .AddJavaScriptEngine()
            .Configure(configure)
            .BuildServiceProvider();

        return new JavaScriptWorkflowScriptEvaluator(
            serviceProvider.GetRequiredService<IScriptingManager>(),
            [],
            new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object);
    }

    private static WorkflowExecutionContext CreateWorkflowContext()
        => new(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

    private sealed class ThrowingScriptingManager : IScriptingManager
    {
        private readonly Exception _exception;

        public ThrowingScriptingManager(Exception exception)
        {
            _exception = exception;
        }

        public IReadOnlyList<IGlobalMethodProvider> GlobalMethodProviders { get; } = [];

        public IScriptingEngine GetScriptingEngine(string prefix) => null;

        public object Evaluate(string directive, IFileProvider fileProvider, string basePath, IEnumerable<IGlobalMethodProvider> scopedMethodProviders)
            => throw _exception;

        public Task<object> EvaluateAsync(string directive, IFileProvider fileProvider, string basePath, IEnumerable<IGlobalMethodProvider> scopedMethodProviders, CancellationToken cancellationToken = default)
            => Task.FromException<object>(_exception);
    }
}
