using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class ExecuteWorkflowTaskDisplayDriver : ActivityDisplayDriver<ExecuteWorkflowTask, ExecuteWorkflowTaskViewModel>
{
    private readonly IWorkflowTypeStore _workflowTypeStore;
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public ExecuteWorkflowTaskDisplayDriver(
        IWorkflowTypeStore workflowTypeStore,
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<ExecuteWorkflowTaskDisplayDriver> stringLocalizer)
    {
        _workflowTypeStore = workflowTypeStore;
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override async ValueTask EditActivityAsync(ExecuteWorkflowTask activity, ExecuteWorkflowTaskViewModel model)
    {
        model.WorkflowTypeId = activity.WorkflowTypeId;
        model.WaitForCompletion = activity.WaitForCompletion;
        model.Workflows = (await _workflowTypeStore.ListAsync())
            .Where(workflowType => workflowType.IsActivity || workflowType.WorkflowTypeId == activity.WorkflowTypeId)
            .OrderBy(workflowType => workflowType.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var selected = model.Workflows.FirstOrDefault(workflowType => workflowType.WorkflowTypeId == activity.WorkflowTypeId);
        var inputs = activity.Inputs;

        model.Inputs = selected?.Variables
            .Where(variable => variable.IsInput)
            .Select(variable => new ExecuteWorkflowInputViewModel
            {
                Name = variable.Name,
                TypeName = variable.TypeName,
                Description = variable.Description,
                Value = WorkflowExpressionInput.From(inputs.FirstOrDefault(entry => string.Equals(entry.Key, variable.Name, StringComparison.OrdinalIgnoreCase)).Value),
            })
            .ToList() ?? [];
    }

    public override async Task<IDisplayResult> UpdateAsync(ExecuteWorkflowTask activity, UpdateEditorContext context)
    {
        var model = new ExecuteWorkflowTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var workflowType = string.IsNullOrEmpty(model.WorkflowTypeId) ? null : await _workflowTypeStore.GetAsync(model.WorkflowTypeId);

        if (workflowType is null || !workflowType.IsActivity)
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.WorkflowTypeId), S["Select a workflow usable as an activity."]);

            return Edit(activity, context);
        }

        activity.WorkflowTypeId = workflowType.WorkflowTypeId;
        activity.WaitForCompletion = model.WaitForCompletion;

        // The inputs posted for another workflow are kept when the selected one has inputs of the same name.
        var inputs = new Dictionary<string, WorkflowExpression<object>>(StringComparer.OrdinalIgnoreCase);

        foreach (var variable in workflowType.Variables.Where(variable => variable.IsInput))
        {
            var index = model.Inputs.FindIndex(input => string.Equals(input?.Name, variable.Name, StringComparison.OrdinalIgnoreCase));

            if (index < 0 || string.IsNullOrWhiteSpace(model.Inputs[index].Value?.Expression))
            {
                continue;
            }

            inputs[variable.Name] = _expressionValidator.Validate<object>(
                model.Inputs[index].Value,
                context.Updater.ModelState,
                Prefix,
                $"{nameof(model.Inputs)}[{index}].{nameof(ExecuteWorkflowInputViewModel.Value)}",
                new WorkflowExpressionInputOptions { Label = variable.Name });
        }

        activity.Inputs = inputs;

        // The task's outputs, for the output bindings, without loading the workflow.
        activity.Outputs = workflowType.Variables
            .Where(variable => variable.IsOutput)
            .Select(variable => new ExecuteWorkflowOutput
            {
                Name = variable.Name,
                TypeName = variable.TypeName,
                Description = variable.Description,
            })
            .ToList();

        return Edit(activity, context);
    }
}
