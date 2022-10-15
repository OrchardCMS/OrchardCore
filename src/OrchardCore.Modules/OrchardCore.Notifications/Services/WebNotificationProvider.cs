using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Modules;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Notifications.Services;

public class WebNotificationProvider : INotificationMethodProvider
{
    private readonly IStringLocalizer<WebNotificationProvider> S;
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IClock _clock;
    private readonly ISession _session;
    private readonly ILogger _logger;

    public WebNotificationProvider(IStringLocalizer<WebNotificationProvider> stringLocalizer,
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        IClock clock,
        ISession session,
        ILogger<WebNotificationProvider> logger)
    {
        S = stringLocalizer;
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _clock = clock;
        _session = session;
        _logger = logger;
    }

    public string Method => "Web";

    public LocalizedString Name => S["Web Notifications"];

    public async Task<bool> TrySendAsync(IUser user, INotificationMessage message)
    {
        if (user is not User su)
        {
            return false;
        }

        var notificationMessage = await _contentManager.NewAsync(NotificationConstants.WebNotificationMessageContentType);
        notificationMessage.CreatedUtc = _clock.UtcNow;
        notificationMessage.Author = su.UserName;
        notificationMessage.Owner = su.UserId;
        notificationMessage.Latest = true;
        notificationMessage.Alter<WebNotificationMessagePart>(part =>
        {
            part.Subject = message.Subject;
            part.Body = message.Body;
        });

        _session.Save(notificationMessage);

        return true;
    }
}
