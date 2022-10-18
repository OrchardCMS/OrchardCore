using System.Collections.Generic;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Drivers;

public class NotificationDisplayDriver : DisplayDriver<Notification>
{
    public override IDisplayResult Display(Notification model)
    {
        var results = new List<IDisplayResult>()
        {
            Shape("NotificationsMeta_SummaryAdmin", new NotificationViewModel(model)).Location("SummaryAdmin", "Meta:20"),
            Shape("NotificationsActions_SummaryAdmin", new NotificationViewModel(model)).Location("SummaryAdmin", "Actions:5"),
            Shape("NotificationsButtonActions_SummaryAdmin", new NotificationViewModel(model)).Location("SummaryAdmin", "ActionsMenu:10")
        };

        return Combine(results);
    }
}
