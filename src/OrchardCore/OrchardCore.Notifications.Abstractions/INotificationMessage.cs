namespace OrchardCore.Notifications;

public interface INotificationMessage
{
    string Summary { get; }

    string TextBody { get; }

    string HtmlBody { get; }

    bool IsHtmlPreferred { get; }
}
