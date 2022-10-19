using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class NotificationMethodProviderAccessor : INotificationMethodProviderAccessor
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;

    public NotificationMethodProviderAccessor(IEnumerable<INotificationMethodProvider> notificationMethodProviders)
    {
        _notificationMethodProviders = notificationMethodProviders;
    }

    public Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(IUser user)
    {
        if (user is User su)
        {
            var notificationPart = su.As<UserNotificationPreferencesPart>();

            var selectedMethods = ((notificationPart?.Methods) ?? Array.Empty<string>()).ToList();
            var optout = notificationPart.Optout ?? Array.Empty<string>();

            if (selectedMethods.Count > 0)
            {
                return Task.FromResult<IEnumerable<INotificationMethodProvider>>(_notificationMethodProviders.Where(provider => !optout.Contains(provider.Method, StringComparer.OrdinalIgnoreCase))
                    // Priority matters to horor user preferences.
                    .OrderBy(provider => selectedMethods.IndexOf(provider.Method))
                    .ThenBy(provider => provider.Name).ToList());
            }

            return Task.FromResult<IEnumerable<INotificationMethodProvider>>(_notificationMethodProviders.Where(provider => !optout.Contains(provider.Method, StringComparer.OrdinalIgnoreCase))
                    .OrderBy(provider => provider.Name).ToList());
        }

        return Task.FromResult(_notificationMethodProviders);
    }
}
