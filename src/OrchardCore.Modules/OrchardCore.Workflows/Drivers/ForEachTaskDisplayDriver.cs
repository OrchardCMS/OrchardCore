using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class ForEachTaskDisplayDriver : ActivityDisplayDriver<ForEachTask, ForEachTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public ForEachTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<ForEachTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(ForEachTask activity, ForEachTaskViewModel model)
    {
        model.Enumerable = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Enumerable, activity.LiquidEnumerable.Expression, activity.Syntax));
        model.LoopVariableName = activity.LoopVariableName;
    }

    public override async Task<IDisplayResult> UpdateAsync(ForEachTask activity, UpdateEditorContext context)
    {
        var model = new ForEachTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.LoopVariableName = model.LoopVariableName?.Trim();
        activity.Enumerable = _expressionValidator.Validate<IEnumerable<object>>(model.Enumerable, context.Updater.ModelState, Prefix, nameof(model.Enumerable), new() { Label = S["Items"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(ForEachTask.LiquidEnumerable));
        activity.Properties.Remove(nameof(ForEachTask.Syntax));

        return Edit(activity, context);
    }
}
