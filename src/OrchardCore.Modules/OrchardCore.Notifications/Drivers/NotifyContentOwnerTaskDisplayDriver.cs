using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Notifications.Activities;

namespace OrchardCore.Notifications.Drivers;

public class NotifyContentOwnerTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyContentOwnerTask>
{
    public NotifyContentOwnerTaskDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<NotifyContentOwnerTaskDisplayDriver> stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, stringLocalizer)
    {
    }
}
