using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Activities;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Facebook.Drivers;

public sealed class MetaConversionsApiEventTaskDisplayDriver : ActivityDisplayDriver<MetaConversionsApiEventTask, MetaConversionsApiEventTaskViewModel>
{
    private readonly ILiquidTemplateManager _liquidTemplateManager;

    internal readonly IStringLocalizer S;

    public MetaConversionsApiEventTaskDisplayDriver(ILiquidTemplateManager liquidTemplateManager, IStringLocalizer<MetaConversionsApiEventTaskDisplayDriver> stringLocalizer)
    {
        _liquidTemplateManager = liquidTemplateManager;
        S = stringLocalizer;
    }

    protected override void EditActivity(MetaConversionsApiEventTask activity, MetaConversionsApiEventTaskViewModel model)
    {
        model.EventName = activity.EventName.Expression;
        model.EventSourceUrl = activity.EventSourceUrl.Expression;
        model.ActionSource = activity.ActionSource;
        model.EventId = activity.EventId.Expression;
    }

    public override async Task<IDisplayResult> UpdateAsync(MetaConversionsApiEventTask activity, UpdateEditorContext context)
    {
        var viewModel = new MetaConversionsApiEventTaskViewModel();

        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        if (string.IsNullOrWhiteSpace(viewModel.EventName))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.EventName), S["Event Name requires a value."]);
        }
        else if (!_liquidTemplateManager.Validate(viewModel.EventName, out var eventNameErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.EventName), string.Join(' ', eventNameErrors));
        }

        if (!string.IsNullOrWhiteSpace(viewModel.EventSourceUrl) && !_liquidTemplateManager.Validate(viewModel.EventSourceUrl, out var urlErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.EventSourceUrl), string.Join(' ', urlErrors));
        }

        if (!string.IsNullOrWhiteSpace(viewModel.EventId) && !_liquidTemplateManager.Validate(viewModel.EventId, out var eventIdErrors))
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.EventId), string.Join(' ', eventIdErrors));
        }

        activity.EventName = new WorkflowExpression<string>(viewModel.EventName);
        activity.EventSourceUrl = new WorkflowExpression<string>(viewModel.EventSourceUrl);
        activity.ActionSource = viewModel.ActionSource;
        activity.EventId = new WorkflowExpression<string>(viewModel.EventId);

        return await EditAsync(activity, context);
    }
}
