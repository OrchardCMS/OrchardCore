using OrchardCore.Scripting;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Scripting;

public class WorkflowMethodsProvider : IGlobalMethodProvider
{
    private readonly GlobalMethod _workflowMethod;
    private readonly GlobalMethod _workflowIdMethod;
    private readonly GlobalMethod _inputMethod;
    private readonly GlobalMethod _outputMethod;
    private readonly GlobalMethod _propertyMethod;
    private readonly GlobalMethod _setPropertyMethod;
    private readonly GlobalMethod _resultMethod;
    private readonly GlobalMethod _correlationIdMethod;
    private readonly GlobalMethod _setCorrelationIdMethod;

    public WorkflowMethodsProvider(WorkflowExecutionContext workflowContext)
    {
        _workflowMethod = new GlobalMethod
        {
            Name = "workflow",
            Method = serviceProvider => (Func<object>)(() => workflowContext),
        };

        _workflowIdMethod = new GlobalMethod
        {
            Name = "workflowId",
            Method = serviceProvider => () => workflowContext.Workflow.WorkflowId,
        };

        _inputMethod = new GlobalMethod
        {
            Name = "input",
            Method = serviceProvider => (Func<string, object>)((name) => workflowContext.Input.TryGetValue(name, out var value) ? value : null),
        };

        _outputMethod = new GlobalMethod
        {
            Name = "output",
            Method = serviceProvider => (Action<string, object>)((name, value) => workflowContext.Output[name] = value),
        };

        _propertyMethod = new GlobalMethod
        {
            Name = "property",
            Method = serviceProvider => (Func<string, object>)((name) => workflowContext.Properties.TryGetValue(name, out var prop) ? prop : null),
        };

        _setPropertyMethod = new GlobalMethod
        {
            Name = "setProperty",
            Method = serviceProvider => (Action<string, object>)((name, value) => workflowContext.Properties[name] = value),
        };

        _resultMethod = new GlobalMethod
        {
            Name = "lastResult",
            Method = serviceProvider => () => workflowContext.LastResult,
        };

        _correlationIdMethod = new GlobalMethod
        {
            Name = "correlationId",
            Method = serviceProvider => () => workflowContext.Workflow.CorrelationId,
        };

        _setCorrelationIdMethod = new GlobalMethod
        {
            Name = "setCorrelationId",
            Method = serviceProvider => ((string correlationId) => workflowContext.CorrelationId = correlationId),
        };

    }

    public IEnumerable<GlobalMethod> GetMethods()
    {
        return [_workflowMethod, _workflowIdMethod, _inputMethod, _outputMethod, _propertyMethod, _resultMethod, _correlationIdMethod, _setPropertyMethod, _setCorrelationIdMethod];
    }
}
