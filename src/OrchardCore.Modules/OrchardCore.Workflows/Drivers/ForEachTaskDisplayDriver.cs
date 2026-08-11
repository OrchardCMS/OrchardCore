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

public sealed class ForEachTaskDisplayDriver : ActivityDisplayDriver<ForEachTask, ForEachTaskViewModel>
{
    private readonly ILiquidTemplateManager _templateManager;
    private readonly IStringLocalizer S;

    public ForEachTaskDisplayDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<ForEachTaskDisplayDriver> stringLocalizer)
    {
        _templateManager = templateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(ForEachTask activity, ForEachTaskViewModel model)
    {
        model.EnumerableExpression = activity.Enumerable.Expression;
        model.LiquidEnumerableExpression = activity.LiquidEnumerable.Expression;
        model.LoopVariableName = activity.LoopVariableName;
        model.Syntax = activity.Syntax;
    }

    public override async Task<IDisplayResult> UpdateAsync(ForEachTask activity, UpdateEditorContext context)
    {
        var model = new ForEachTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.LoopVariableName = model.LoopVariableName?.Trim();
        activity.Enumerable = new WorkflowExpression<IEnumerable<object>>(model.EnumerableExpression);
        activity.LiquidEnumerable = new WorkflowExpression<object>(model.LiquidEnumerableExpression);
        activity.Syntax = model.Syntax;

        if (model.Syntax == WorkflowScriptSyntax.Liquid)
        {
            if (string.IsNullOrWhiteSpace(model.LiquidEnumerableExpression))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.LiquidEnumerableExpression), S["Enumerable is required field."]);
            }
            else if (!_templateManager.Validate(model.LiquidEnumerableExpression, out var errors))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.LiquidEnumerableExpression), S["Enumerable doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
            }
        }
        else if (model.Syntax == WorkflowScriptSyntax.JavaScript)
        {
            if (string.IsNullOrWhiteSpace(model.EnumerableExpression))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.EnumerableExpression), S["Enumerable is required field."]);
            }
        }

        return Edit(activity, context);
    }
}
