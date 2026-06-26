using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

public sealed class SetOutputTaskDisplayDriver : ActivityDisplayDriver<SetOutputTask, SetOutputTaskViewModel>
{
    private readonly ILiquidTemplateManager _templateManager;

    private readonly IStringLocalizer S;

    public SetOutputTaskDisplayDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<SetOutputTaskDisplayDriver> stringLocalizer)
    {
        _templateManager = templateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(SetOutputTask source, SetOutputTaskViewModel model)
    {
        model.OutputName = source.OutputName;
        model.Value = source.Value.Expression;
        model.LiquidValue = source.LiquidValue.Expression;
        model.Syntax = source.Syntax;
    }

    public override async Task<IDisplayResult> UpdateAsync(SetOutputTask activity, UpdateEditorContext context)
    {
        var model = new SetOutputTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.Syntax = model.Syntax;
        activity.LiquidValue = new WorkflowExpression<object>(model.LiquidValue);
        activity.OutputName = model.OutputName?.Trim();
        activity.Value = new WorkflowExpression<object>(model.Value);

        if (model.Syntax == WorkflowScriptSyntax.Liquid)
        {
            if (string.IsNullOrWhiteSpace(model.LiquidValue))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.LiquidValue), S["Value is required field."]);
            }
            else if (!_templateManager.Validate(model.LiquidValue, out var errors))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.LiquidValue), S["Value doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
        }
        else if (model.Syntax == WorkflowScriptSyntax.JavaScript)
        {
            if (string.IsNullOrWhiteSpace(model.Value))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.Value), S["Value is required field."]);
            }
        }

        return Edit(activity, context);
    }
}
