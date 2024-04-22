using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.Drivers;

public class NotifyUserTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyUserTask>
{
    public NotifyUserTaskDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer<NotifyUserTaskDisplayDriver> stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, notificationOptions, stringLocalizer)
    {
    }
}
