using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class SetVariableTaskDisplayDriver : ActivityDisplayDriver<SetVariableTask, SetVariableTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public SetVariableTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<SetVariableTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(SetVariableTask activity, SetVariableTaskViewModel model)
    {
        model.VariableName = activity.VariableName;
        model.Value = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Value, activity.LiquidValue.Expression, activity.Syntax));
    }

    public override async Task<IDisplayResult> UpdateAsync(SetVariableTask activity, UpdateEditorContext context)
    {
        var model = new SetVariableTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.VariableName = model.VariableName?.Trim();
        activity.Value = _expressionValidator.Validate<object>(model.Value, context.Updater.ModelState, Prefix, nameof(model.Value), new() { Label = S["Value"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(SetVariableTask.LiquidValue));
        activity.Properties.Remove(nameof(SetVariableTask.Syntax));

        return Edit(activity, context);
    }
}
