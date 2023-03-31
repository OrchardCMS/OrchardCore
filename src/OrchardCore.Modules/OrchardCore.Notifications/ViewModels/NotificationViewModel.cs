using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Notifications.ViewModels;

public class NotificationViewModel : ShapeViewModel
{
    public Notification Notification { get; set; }

    public NotificationViewModel()
    {
    }

    public NotificationViewModel(Notification notification)
    {
        Notification = notification;
    }
}
