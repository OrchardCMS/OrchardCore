using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Text;
using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class NotificationManager : INotificationManager
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;

    public NotificationManager(IEnumerable<INotificationMethodProvider> notificationMethodProviders)
    {
        _notificationMethodProviders = notificationMethodProviders;
    }

    public async Task<int> SendAsync(IUser user, INotificationMessage message)
    {
        var totalSent = 0;

        if (user is User su)
        {
            var notificationPart = su.As<UserNotificationPart>();

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
}
