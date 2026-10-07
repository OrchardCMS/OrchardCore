using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class SetPropertyTaskDisplayDriver : ActivityDisplayDriver<SetPropertyTask, SetPropertyTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public SetPropertyTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<SetPropertyTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(SetPropertyTask activity, SetPropertyTaskViewModel model)
    {
        model.PropertyName = activity.PropertyName;
        model.Value = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Value, activity.LiquidValue.Expression, activity.Syntax));
    }

    public override async Task<IDisplayResult> UpdateAsync(SetPropertyTask activity, UpdateEditorContext context)
    {
        var model = new SetPropertyTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.PropertyName = model.PropertyName?.Trim();
        activity.Value = _expressionValidator.Validate<object>(model.Value, context.Updater.ModelState, Prefix, nameof(model.Value), new() { Label = S["Value"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(SetPropertyTask.LiquidValue));
        activity.Properties.Remove(nameof(SetPropertyTask.Syntax));

        return Edit(activity, context);
    }
}
