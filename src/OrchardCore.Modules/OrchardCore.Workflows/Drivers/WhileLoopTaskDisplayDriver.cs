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

public sealed class WhileLoopTaskDisplayDriver : ActivityDisplayDriver<WhileLoopTask, WhileLoopTaskViewModel>
{
    private readonly ILiquidTemplateManager _templateManager;
    private readonly IStringLocalizer S;

    public WhileLoopTaskDisplayDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<WhileLoopTaskDisplayDriver> stringLocalizer)
    {
        _templateManager = templateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(WhileLoopTask source, WhileLoopTaskViewModel model)
    {
        model.ConditionExpression = source.Condition.Expression;
        model.LiquidConditionExpression = source.LiquidCondition.Expression;
        model.Syntax = source.Syntax;
    }

    public override async Task<IDisplayResult> UpdateAsync(WhileLoopTask activity, UpdateEditorContext context)
    {
        var model = new WhileLoopTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.Condition = new WorkflowExpression<bool>(model.ConditionExpression?.Trim());
        activity.LiquidCondition = new WorkflowExpression<bool>(model.LiquidConditionExpression);
        activity.Syntax = model.Syntax;

        if (model.Syntax == WorkflowScriptSyntax.Liquid)
        {
            if (string.IsNullOrWhiteSpace(model.LiquidConditionExpression))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.LiquidConditionExpression), S["Condition is required field."]);
            }
            else if (!_templateManager.Validate(model.LiquidConditionExpression, out var errors))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.LiquidConditionExpression), S["Condition doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
        }
        else if (model.Syntax == WorkflowScriptSyntax.JavaScript)
        {
            if (string.IsNullOrWhiteSpace(model.ConditionExpression))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConditionExpression), S["Condition is required field."]);
            }
        }

        return Edit(activity, context);
    }
}
