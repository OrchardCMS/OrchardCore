using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Activities;

/// <summary>
/// Runs another workflow, one that is usable as an activity (<see cref="WorkflowType.IsActivity"/>), with its input
/// variables set from expressions, and gets the values of its output variables back as the task's outputs.
/// </summary>
public class ExecuteWorkflowTask : TaskActivity<ExecuteWorkflowTask>, IActivityOutputs, IActivityProvidedValues
{
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly IWorkflowExpressionManager _expressionManager;
    private readonly IServiceProvider _serviceProvider;
    protected readonly IStringLocalizer S;

    public ExecuteWorkflowTask(
        IWorkflowTypeStore workflowTypeStore,
        IWorkflowExpressionManager expressionManager,
        IServiceProvider serviceProvider,
        IStringLocalizer<ExecuteWorkflowTask> localizer)
    {
        _workflowTypeStore = workflowTypeStore;
        _expressionManager = expressionManager;
        _serviceProvider = serviceProvider;
        S = localizer;
    }

    public override LocalizedString DisplayText => S["Execute Workflow Task"];

    public override LocalizedString Category => S["Primitives"];

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/> of the workflow to run.
    /// </summary>
    public string WorkflowTypeId
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    /// <summary>
    /// The expressions of the input values, by the name of the input variable they set. An expression without a
    /// syntax is JavaScript.
    /// </summary>
    public IDictionary<string, WorkflowExpression<object>> Inputs
    {
        get => GetProperty(() => new Dictionary<string, WorkflowExpression<object>>());
        set => SetProperty(value);
    }

    /// <summary>
    /// Whether the task waits for the workflow to finish and gets its outputs. Otherwise it takes its <c>Done</c>
    /// outcome once the workflow started, even when the workflow waits on an event.
    /// </summary>
    public bool WaitForCompletion
    {
        get => GetProperty(() => true);
        set => SetProperty(value);
    }

    /// <summary>
    /// The output variables of the workflow, stored when the task is edited, so the designer knows the task's
    /// outputs without loading the workflow.
    /// </summary>
    public IList<ExecuteWorkflowOutput> Outputs
    {
        get => GetProperty(() => new List<ExecuteWorkflowOutput>());
        set => SetProperty(value);
    }

    /// <summary>
    /// The instance the task waits on.
    /// </summary>
    public string ChildWorkflowId
    {
        get => GetProperty<string>();
        set => SetProperty(value);
    }

    public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        => Outcome(S["Done"], S["Failed"]);

    public IEnumerable<ActivityOutputDescriptor> GetOutputs()
        => Outputs.Select(output => new ActivityOutputDescriptor
        {
            Name = output.Name,
            TypeName = string.IsNullOrEmpty(output.TypeName) ? "any" : output.TypeName,
            DisplayName = new LocalizedString(output.Name, string.IsNullOrEmpty(output.Description) ? output.Name : output.Description),
        });

    public IEnumerable<ActivityProvidedValue> GetProvidedValues()
        =>
        [
            ActivityProvidedValue.LastResult(
                "object",
                S["The outputs of the workflow it ran, by name, or its fault message when it failed."],
                Outputs
                    .Select(output => new ActivityProvidedValueMember
                    {
                        Name = output.Name,
                        TypeName = string.IsNullOrEmpty(output.TypeName) ? "any" : output.TypeName,
                        Description = new LocalizedString(output.Name, output.Description ?? string.Empty),
                    })
                    .ToList()),
        ];

    public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        var workflowType = string.IsNullOrEmpty(WorkflowTypeId) ? null : await _workflowTypeStore.GetAsync(WorkflowTypeId);

        if (workflowType is null || !workflowType.IsActivity)
        {
            throw new InvalidOperationException($"The workflow '{WorkflowTypeId}' doesn't exist, or isn't usable as an activity.");
        }

        if (!workflowType.IsEnabled)
        {
            throw new InvalidOperationException($"The workflow '{workflowType.Name}' is disabled.");
        }

        var input = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var inputs = Inputs;

        foreach (var variable in workflowType.Variables.Where(variable => variable.IsInput))
        {
            var expression = inputs.FirstOrDefault(entry => string.Equals(entry.Key, variable.Name, StringComparison.OrdinalIgnoreCase)).Value;

            if (!string.IsNullOrWhiteSpace(expression?.Expression))
            {
                input[variable.Name] = await _expressionManager.EvaluateAsync(expression, workflowContext, WorkflowExpressionSyntaxes.JavaScript);
            }
        }

        // Resolved here: the workflow manager creates the activities.
        var workflowManager = _serviceProvider.GetRequiredService<IWorkflowManager>();
        var child = await workflowManager.StartChildWorkflowAsync(workflowType, workflowContext, activityContext.ActivityRecord.ActivityId, input);

        if (!WaitForCompletion)
        {
            return Outcome("Done");
        }

        if (child.Status == WorkflowStatus.Halted)
        {
            ChildWorkflowId = child.WorkflowId;

            return Halt();
        }

        return Complete(workflowContext, activityContext, ChildWorkflowResult.From(child));
    }

    public override ActivityExecutionResult Resume(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
    {
        // The task only continues with the result of the instance it waits on.
        if (!workflowContext.Input.TryGetValue(ChildWorkflowResult.InputKey, out var value) ||
            value is not ChildWorkflowResult result ||
            string.IsNullOrEmpty(ChildWorkflowId) ||
            result.WorkflowId != ChildWorkflowId)
        {
            return Halt();
        }

        // The result isn't kept in the instance's input.
        workflowContext.Input.Remove(ChildWorkflowResult.InputKey);
        ChildWorkflowId = null;

        return Complete(workflowContext, activityContext, result);
    }

    private static ActivityExecutionResult Complete(WorkflowExecutionContext workflowContext, ActivityContext activityContext, ChildWorkflowResult result)
    {
        if (result.Status != WorkflowStatus.Finished)
        {
            workflowContext.LastResult = result.FaultMessage;

            return Outcome("Failed");
        }

        foreach (var (name, value) in result.Outputs)
        {
            workflowContext.SetActivityOutput(activityContext, name, value);
        }

        workflowContext.LastResult = result.Outputs;

        return Outcome("Done");
    }
}

/// <summary>
/// An output variable of the workflow an Execute Workflow task runs.
/// </summary>
public sealed class ExecuteWorkflowOutput
{
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The name of its type.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Its description, shown as the output's name in the designer.
    /// </summary>
    public string Description { get; set; }
}
