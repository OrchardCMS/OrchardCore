using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Models;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotifyContentOwnerTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyContentOwnerTask>
{
    public NotifyContentOwnerTaskDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IOptions<NotificationOptions> notificationOptions,
        IStringLocalizer<NotifyContentOwnerTaskDisplayDriver> stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, notificationOptions, stringLocalizer)
    {
    }
}
