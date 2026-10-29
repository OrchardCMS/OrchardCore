using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class CorrelateTaskDisplayDriver : ActivityDisplayDriver<CorrelateTask, CorrelateTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public CorrelateTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<CorrelateTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(CorrelateTask activity, CorrelateTaskViewModel model)
    {
        model.Value = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Value, activity.Value.Expression, activity.Syntax));
    }

    public override async Task<IDisplayResult> UpdateAsync(CorrelateTask activity, UpdateEditorContext context)
    {
        var model = new CorrelateTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.Value = _expressionValidator.Validate<string>(model.Value, context.Updater.ModelState, Prefix, nameof(model.Value), new() { Label = S["Value"], Required = false });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(CorrelateTask.Syntax));

        return Edit(activity, context);
    }
}
