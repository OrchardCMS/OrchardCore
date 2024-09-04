using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotifyUserTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyUserTask, NotifyUserTaskViewModel>
{
    public NotifyUserTaskDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer<NotifyUserTaskDisplayDriver> stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, notificationOptions, stringLocalizer)
    {
    }

    protected override string EditShapeType { get; } = $"{ActivityName}_Fields_Edit";

    public override async Task<IDisplayResult> UpdateAsync(NotifyUserTask activity, UpdateEditorContext context)
    {
        var viewModel = new NotifyUserTaskViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix);

        var userNames = viewModel.UserNames?.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase) ?? [];

        if (!userNames.Any())
        {
            context.Updater.ModelState.AddModelError(Prefix, nameof(viewModel.UserNames), S["Please provide at least one username to notify."]);
        }
        else
        {
            activity.UserNames = new WorkflowExpression<string>(string.Join(", ", userNames));
        }

        return await base.UpdateAsync(activity, context);
    }

    protected override void EditActivity(NotifyUserTask activity, NotifyUserTaskViewModel model)
    {
        base.EditActivity(activity, model);

        model.UserNames = activity.UserNames.Expression;
    }
}
