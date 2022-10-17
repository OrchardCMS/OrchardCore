using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Notifications.Services;

public class WebNotificationProvider : INotificationMethodProvider
{
    private readonly IStringLocalizer<WebNotificationProvider> S;
    private readonly IContentManager _contentManager;
    private readonly IClock _clock;
    private readonly ISession _session;

    public WebNotificationProvider(IStringLocalizer<WebNotificationProvider> stringLocalizer,
        IContentManager contentManager,
        IClock clock,
        ISession session)
    {
        S = stringLocalizer;
        _contentManager = contentManager;
        _clock = clock;
        _session = session;
    }

    public string Method => "Web";

    public LocalizedString Name => S["Web Notifications"];

    public Task<bool> TrySendAsync(IUser user, INotificationMessage message)
    {
        if (user is not User su)
        {
            return Task.FromResult(false);
        }

        var notification = new WebNotification()
        {
            NotificationId = IdGenerator.GenerateId(),
            CreatedUtc = _clock.UtcNow,
            UserId = su.UserId,
            Subject = message.Subject,
            Body = message.Body,
            IsHtmlBody = message is HtmlNotificationMessage nm && nm.BodyContainsHtml
        };

        _session.Save(notification, collection: NotificationConstants.NotificationCollection);

        return Task.FromResult(true);
    }
}
