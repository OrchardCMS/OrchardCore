using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class ForLoopTaskDisplayDriver : ActivityDisplayDriver<ForLoopTask, ForLoopTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public ForLoopTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<ForLoopTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(ForLoopTask activity, ForLoopTaskViewModel model)
    {
        model.From = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.From, activity.LiquidFrom.Expression, activity.Syntax));
        model.To = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.To, activity.LiquidTo.Expression, activity.Syntax));
        model.Step = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Step, activity.LiquidStep.Expression, activity.Syntax));
        model.LoopVariableName = activity.LoopVariableName;
    }

    public override async Task<IDisplayResult> UpdateAsync(ForLoopTask activity, UpdateEditorContext context)
    {
        var model = new ForLoopTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.LoopVariableName = model.LoopVariableName?.Trim();
        activity.From = _expressionValidator.Validate<double>(model.From, context.Updater.ModelState, Prefix, nameof(model.From), new() { Label = S["From"], Required = true });
        activity.To = _expressionValidator.Validate<double>(model.To, context.Updater.ModelState, Prefix, nameof(model.To), new() { Label = S["To"], Required = true });
        activity.Step = _expressionValidator.Validate<double>(model.Step, context.Updater.ModelState, Prefix, nameof(model.Step), new() { Label = S["Step"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(ForLoopTask.LiquidFrom));
        activity.Properties.Remove(nameof(ForLoopTask.LiquidTo));
        activity.Properties.Remove(nameof(ForLoopTask.LiquidStep));
        activity.Properties.Remove(nameof(ForLoopTask.Syntax));

        return Edit(activity, context);
    }
}
