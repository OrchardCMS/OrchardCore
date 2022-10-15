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

    public async Task<bool> TrySendAsync(IUser user, INotificationMessage message)
    {
        if (user is not User su)
        {
            return false;
        }

        var notificationMessage = await _contentManager.NewAsync(NotificationConstants.WebNotificationContentType);
        notificationMessage.CreatedUtc = _clock.UtcNow;
        notificationMessage.Author = su.UserName;
        notificationMessage.Owner = su.UserId;
        notificationMessage.Latest = true;
        notificationMessage.Alter<WebNotificationPart>(part =>
        {
            part.Subject = message.Subject;
            part.Body = message.Body;
            part.IsHtmlBody = message is HtmlNotificationMessage nm && nm.BodyContainsHtml;
        });

        _session.Save(notificationMessage);

        return true;
    }
}
