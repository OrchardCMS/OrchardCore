using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using YesSql;

namespace OrchardCore.Notifications.Services;

public class NotificationManager : INotificationManager
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;
    private readonly ISession _session;
    private readonly IClock _clock;

    public NotificationManager(IEnumerable<INotificationMethodProvider> notificationMethodProviders,
        ISession session,
        IClock clock)
    {
        _notificationMethodProviders = notificationMethodProviders;
        _session = session;
        _clock = clock;
    }

    public async Task<int> SendAsync(IUser user, INotificationMessage message)
    {
        var totalSent = 0;

        if (user is User su)
        {
            SaveNotification(message, su);

            var notificationPart = su.As<UserNotificationPreferencesPart>();

            var selectedMethods = ((notificationPart?.Methods) ?? Array.Empty<string>()).ToList();
            var optout = notificationPart.Optout ?? Array.Empty<string>();

            var allowedProviders = _notificationMethodProviders.Where(provider => !optout.Contains(provider.Method, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(provider => provider.Name);

            if (selectedMethods.Count > 0)
            {
                allowedProviders = _notificationMethodProviders.Where(provider => !optout.Contains(provider.Method, StringComparer.OrdinalIgnoreCase))
                    // Priority matters to horor user preferences.
                    .OrderBy(provider => selectedMethods.IndexOf(provider.Method))
                    .ThenBy(provider => provider.Name);
            }

            foreach (var allowedProvider in allowedProviders)
            {
                if (await allowedProvider.TrySendAsync(user, message))
                {
                    totalSent++;

                    if (notificationPart.Strategy == UserNotificationStrategy.UntilFirstSuccess)
                    {
                        // Since one notification was sent, we no longer need to send using other services.
                        break;
                    }
                }
            }
        }

        return totalSent;
    }

    private void SaveNotification(INotificationMessage message, User su)
    {
        var notification = new Notification()
        {
            NotificationId = IdGenerator.GenerateId(),
            CreatedUtc = _clock.UtcNow,
            UserId = su.UserId,
            Subject = message.Subject,
            Body = message.Body,
            IsHtmlBody = message is HtmlNotificationMessage nm && nm.BodyContainsHtml
        };

        if (message is ContentNotificationMessage contentMessage)
        {
            if (!String.IsNullOrEmpty(contentMessage.ContentItemId))
            {
                notification.ContentItemId = contentMessage.ContentItemId;
            }
            else if (!String.IsNullOrWhiteSpace(contentMessage.Url))
            {
                notification.Url = contentMessage.Url;
            }
        }
        _session.Save(notification, collection: NotificationConstants.NotificationCollection);
    }
}
