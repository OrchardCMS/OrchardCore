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

public sealed class LiquidTaskDisplayDriver : ActivityDisplayDriver<LiquidTask, LiquidTaskViewModel>
{
    private readonly ILiquidTemplateManager _templateManager;
    private readonly IStringLocalizer S;

    public LiquidTaskDisplayDriver(
        ILiquidTemplateManager templateManager,
        IStringLocalizer<LiquidTaskDisplayDriver> stringLocalizer)
    {
        _templateManager = templateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(LiquidTask source, LiquidTaskViewModel model)
    {
        model.Expression = source.Expression.Expression;
    }

    public override async Task<IDisplayResult> UpdateAsync(LiquidTask activity, UpdateEditorContext context)
    {
        var model = new LiquidTaskViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        activity.Expression = new WorkflowExpression<object>(model.Expression);

        if (string.IsNullOrWhiteSpace(model.Expression))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Expression), S["Expression is required field."]);
        }
        else if (!_templateManager.Validate(model.Expression, out var errors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(model.Expression), S["Expression doesn't contain a valid Liquid expression. Details: {0}", string.Join(" ", errors)]);
        }

        return Edit(activity, context);
    }
}
