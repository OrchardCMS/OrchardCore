using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class SetOutputTaskDisplayDriver : ActivityDisplayDriver<SetOutputTask, SetOutputTaskViewModel>
{
    private readonly WorkflowExpressionInputValidator _expressionValidator;

    internal readonly IStringLocalizer S;

    public SetOutputTaskDisplayDriver(
        WorkflowExpressionInputValidator expressionValidator,
        IStringLocalizer<SetOutputTaskDisplayDriver> stringLocalizer)
    {
        _expressionValidator = expressionValidator;
        S = stringLocalizer;
    }

    protected override void EditActivity(SetOutputTask activity, SetOutputTaskViewModel model)
    {
        model.OutputName = activity.OutputName;
        model.Value = WorkflowExpressionInput.From(WorkflowExpressionSyntaxes.Resolve(activity.Value, activity.LiquidValue.Expression, activity.Syntax));
    }

    public override async Task<IDisplayResult> UpdateAsync(SetOutputTask activity, UpdateEditorContext context)
    {
        var model = new SetOutputTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.OutputName = model.OutputName?.Trim();
        activity.Value = _expressionValidator.Validate<object>(model.Value, context.Updater.ModelState, Prefix, nameof(model.Value), new() { Label = S["Value"], Required = true });

        // The expressions carry their syntax now, so the properties of the former syntax setting go.
        activity.Properties.Remove(nameof(SetOutputTask.LiquidValue));
        activity.Properties.Remove(nameof(SetOutputTask.Syntax));

        return Edit(activity, context);
    }
}
