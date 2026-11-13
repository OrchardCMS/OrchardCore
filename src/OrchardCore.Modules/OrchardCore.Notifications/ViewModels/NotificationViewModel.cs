using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Notifications.ViewModels;

public class NotificationViewModel : ShapeViewModel
{
    public Notification Notification { get; set; }

    /// <summary>
    /// Gets or sets whether the notification has been read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Gets or sets how the notification is shown to the current user.
    /// </summary>
    public NotificationVisibility Visibility { get; set; }

    /// <summary>
    /// Gets or sets the registered type of the notification, or <see langword="null"/>.
    /// </summary>
    public NotificationTypeDescriptor TypeDescriptor { get; set; }

    public NotificationViewModel()
    {
    }

    public NotificationViewModel(Notification notification)
    {
        Notification = notification;
    }
}
