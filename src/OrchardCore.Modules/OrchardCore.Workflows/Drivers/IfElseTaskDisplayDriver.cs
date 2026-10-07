using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class IfElseTaskDisplayDriver : ActivityDisplayDriver<IfElseTask, IfElseTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public IfElseTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<IfElseTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(IfElseTask activity, IfElseTaskViewModel model)
    {
        model.Condition = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Condition, activity.LiquidCondition.Expression, activity.Syntax));
    }

    public override async Task<IDisplayResult> UpdateAsync(IfElseTask activity, UpdateEditorContext context)
    {
        var model = new IfElseTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.Condition = _expressionValidator.Validate<bool>(model.Condition, context.Updater.ModelState, Prefix, nameof(model.Condition), new() { Label = S["Condition"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(IfElseTask.LiquidCondition));
        activity.Properties.Remove(nameof(IfElseTask.Syntax));

        return Edit(activity, context);
    }
}
