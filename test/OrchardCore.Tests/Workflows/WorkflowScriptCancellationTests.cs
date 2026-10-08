using OrchardCore.Scripting;
using OrchardCore.Workflows.Evaluators;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Workflows;

public class WorkflowScriptCancellationTests
{
    [Fact]
    public async Task EvaluateAsync_PassesTheWorkflowCancellationTokenToTheScript()
    {
        var scriptingManager = new TokenRecordingScriptingManager();
        var evaluator = new JavaScriptWorkflowScriptEvaluator(
            scriptingManager,
            [],
            new Mock<ILogger<JavaScriptWorkflowScriptEvaluator>>().Object);

        using var workflowContext = new WorkflowExecutionContext(
            new WorkflowType(),
            new Workflow { WorkflowId = IdGenerator.GenerateId() },
            null,
            null,
            null,
            null,
            null,
            []);

        await evaluator.EvaluateAsync(new WorkflowExpression<object>("1 + 1"), workflowContext);

        // A workflow that is cancelled has to stop a script it is running, which the scripting manager can
        // only do if it is handed the token the workflow is cancelled through.
        Assert.True(scriptingManager.CancellationToken.CanBeCanceled);
        Assert.Equal(workflowContext.CancellationToken, scriptingManager.CancellationToken);
    }

    private sealed class TokenRecordingScriptingManager : IScriptingManager
    {
        public CancellationToken CancellationToken { get; private set; }

        public IReadOnlyList<IGlobalMethodProvider> GlobalMethodProviders { get; } = [];

        public IScriptingEngine GetScriptingEngine(string prefix) => null;

        public object Evaluate(string directive, IFileProvider fileProvider, string basePath, IEnumerable<IGlobalMethodProvider> scopedMethodProviders)
            => throw new NotSupportedException();

        public Task<object> EvaluateAsync(string directive, IFileProvider fileProvider, string basePath, IEnumerable<IGlobalMethodProvider> scopedMethodProviders, CancellationToken cancellationToken = default)
        {
            CancellationToken = cancellationToken;

            return Task.FromResult<object>(null);
        }
    }
}
