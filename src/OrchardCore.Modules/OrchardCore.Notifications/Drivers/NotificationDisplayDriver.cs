using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public sealed class NotificationDisplayDriver : DisplayDriver<Notification>
{
    public override Task<IDisplayResult> DisplayAsync(Notification notification, BuildDisplayContext context)
    {
        return CombineAsync(
            Shape("NotificationsMeta_SummaryAdmin", new NotificationViewModel(notification))
                .Location("SummaryAdmin", "Meta:20"),
            Shape("NotificationsActions_SummaryAdmin", new NotificationViewModel(notification))
                .Location("SummaryAdmin", "Actions:5"),
            Shape("NotificationsButtonActions_SummaryAdmin", new NotificationViewModel(notification))
                .Location("SummaryAdmin", "ActionsMenu:10")
        );
    }
}
