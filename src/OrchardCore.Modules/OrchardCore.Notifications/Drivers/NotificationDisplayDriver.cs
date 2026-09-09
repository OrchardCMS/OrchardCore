using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationDisplayDriver : DisplayDriver<Notification>
{
    public override Task<IDisplayResult> DisplayAsync(Notification notification, BuildDisplayContext context)
    {
        return CombineAsync(
            Factory("NotificationsCheckbox_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            Factory("NotificationsSubject_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            Factory("NotificationsMeta_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:20"),
            Factory("NotificationsActions_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            Factory("NotificationsButtonActions_SummaryAdmin", static (Notification n) => new NotificationViewModel(n), notification)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:10")
        );
    }
}
