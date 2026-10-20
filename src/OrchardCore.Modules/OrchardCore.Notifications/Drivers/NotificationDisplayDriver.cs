using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationDisplayDriver : DisplayDriver<Notification>
{
    public override Task<IDisplayResult> DisplayAsync(Notification notification, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            rowShape.Properties[nameof(Notification)] = notification;
            rowShape.Classes.Add(notification.GetOrCreate<NotificationReadInfo>().IsRead
                ? NotificationsAdminList.ReadCssClass
                : NotificationsAdminList.UnreadCssClass);
        }

        return CombineAsync(
            Factory("NotificationsCheckbox_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            Factory("NotificationsSubject_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Subject:1"),
            Factory("NotificationsMeta_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:20")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Received:20"),
            Factory("NotificationsActions_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            Factory("NotificationsButtonActions_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:10")
        );
    }
}
