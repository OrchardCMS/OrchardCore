using Jint.Runtime;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Evaluators;

public class JavaScriptWorkflowScriptEvaluator : IWorkflowScriptEvaluator
{
    private readonly IScriptingManager _scriptingManager;
    private readonly IEnumerable<IWorkflowExecutionContextHandler> _workflowContextHandlers;
    private readonly ILogger _logger;

    public JavaScriptWorkflowScriptEvaluator(
        IScriptingManager scriptingManager,
        IEnumerable<IWorkflowExecutionContextHandler> workflowContextHandlers,
        ILogger<JavaScriptWorkflowScriptEvaluator> logger
    )
    {
        _scriptingManager = scriptingManager;
        _workflowContextHandlers = workflowContextHandlers;
        _logger = logger;
    }

    public async Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, params IGlobalMethodProvider[] scopedMethodProviders)
    {
        if (string.IsNullOrWhiteSpace(expression.Expression))
        {
            return default;
        }

        try
        {
            var workflowType = workflowContext.WorkflowType;
            var directive = $"js:{expression}";
            var expressionContext = new WorkflowExecutionScriptContext(workflowContext);

            await _workflowContextHandlers.InvokeAsync((h, expressionContext) => h.EvaluatingScriptAsync(expressionContext), expressionContext, _logger);

            var methodProviders = scopedMethodProviders.Concat(expressionContext.ScopedMethodProviders);

            // Some types cannot be cast (e.g., null to bool), so we need to catch the exception and return the default value.
            return (T)await _scriptingManager.EvaluateAsync(directive, null, null, methodProviders, workflowContext.CancellationToken);
        }
        catch (Exception ex) when (!IsStoppedEvaluation(ex))
        {
            _logger.LogError(ex, "An error occurred while evaluating the expression: {Expression}", expression.Expression);
        }

        return default;
    }

    /// <summary>
    /// Whether the evaluation was stopped from outside the script - cancelled, or ended by one of the execution
    /// limits a site can configure on <see cref="Jint.Options"/> - rather than failed because of the script.
    /// </summary>
    /// <remarks>
    /// A script that throws, does not parse or returns a value that cannot be converted keeps falling back to the
    /// default value. A stopped script is different: it never produced a value at all, and substituting one
    /// would send the workflow down an outcome nobody decided on. These exceptions are left to the workflow
    /// manager, which faults the workflow at the activity that was running the script.
    /// </remarks>
    private static bool IsStoppedEvaluation(Exception exception)
        => exception is OperationCanceledException // The cancellation token of the evaluation.
            or TimeoutException // Jint.Options.TimeoutInterval(), or a time budget given to OperationDeadlineConstraint.
            or StatementsCountOverflowException // Jint.Options.MaxStatements().
            or MemoryLimitExceededException // Jint.Options.LimitMemory().
            or RecursionDepthOverflowException // Jint.Options.LimitRecursion().
            or ExecutionCanceledException; // Jint.Options.CancellationToken().
}
