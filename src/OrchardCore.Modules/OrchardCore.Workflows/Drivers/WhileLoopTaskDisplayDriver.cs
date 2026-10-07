using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class WhileLoopTaskDisplayDriver : ActivityDisplayDriver<WhileLoopTask, WhileLoopTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public WhileLoopTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<WhileLoopTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(WhileLoopTask activity, WhileLoopTaskViewModel model)
    {
        model.Condition = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Condition, activity.LiquidCondition.Expression, activity.Syntax));
    }

    public override async Task<IDisplayResult> UpdateAsync(WhileLoopTask activity, UpdateEditorContext context)
    {
        var model = new WhileLoopTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.Condition = _expressionValidator.Validate<bool>(model.Condition, context.Updater.ModelState, Prefix, nameof(model.Condition), new() { Label = S["Condition"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(WhileLoopTask.LiquidCondition));
        activity.Properties.Remove(nameof(WhileLoopTask.Syntax));

        return Edit(activity, context);
    }
}
