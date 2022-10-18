using System.Collections.Generic;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public class WebNotificationDisplayDriver : DisplayDriver<WebNotification>
{
    public override IDisplayResult Display(WebNotification model)
    {
        var results = new List<IDisplayResult>()
        {
            Shape("NotificationsMeta_SummaryAdmin", new WebNotificationViewModel(model)).Location("SummaryAdmin", "Meta:20"),
            Shape("NotificationsActions_SummaryAdmin", new WebNotificationViewModel(model)).Location("SummaryAdmin", "Actions:5"),
            Shape("NotificationsButtonActions_SummaryAdmin", new WebNotificationViewModel(model)).Location("SummaryAdmin", "ActionsMenu:10")
        };

        return Combine(results);
    }
}
