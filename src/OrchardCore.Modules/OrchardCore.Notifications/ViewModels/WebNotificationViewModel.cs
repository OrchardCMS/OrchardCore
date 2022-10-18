using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Notifications.ViewModels;

public class WebNotificationViewModel : ShapeViewModel
{
    public WebNotification Notification { get; set; }

    public WebNotificationViewModel()
    {

    }

    public WebNotificationViewModel(WebNotification notification)
    {
        Notification = notification;
    }

}
