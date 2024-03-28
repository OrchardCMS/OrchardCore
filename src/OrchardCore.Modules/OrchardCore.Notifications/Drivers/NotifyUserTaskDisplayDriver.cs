using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Notifications.Activities;

namespace OrchardCore.Notifications.Drivers;

public class NotifyUserTaskDisplayDriver : NotifyUserTaskActivityDisplayDriver<NotifyUserTask>
{
    public NotifyUserTaskDisplayDriver(
        IHtmlSanitizerService htmlSanitizerService,
        ILiquidTemplateManager liquidTemplateManager,
        IStringLocalizer<NotifyUserTaskDisplayDriver> stringLocalizer)
        : base(htmlSanitizerService, liquidTemplateManager, stringLocalizer)
    {
    }
}
