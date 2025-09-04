using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class NotificationMethodProviderAccessor : INotificationMethodProviderAccessor
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;

    public NotificationMethodProviderAccessor(IEnumerable<INotificationMethodProvider> notificationMethodProviders)
    {
        _notificationMethodProviders = notificationMethodProviders;
    }

    public Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify)
    {
        var user = notify as User;

        if (user != null)
        {
            var notificationPart = user.As<UserNotificationPreferencesPart>();

            var selectedMethods = ((notificationPart?.Methods) ?? []).ToList();
            var optout = notificationPart.Optout ?? [];

            var methods = _notificationMethodProviders.Where(provider => !optout.Contains(provider.Method));
            if (selectedMethods.Count > 0)
            {
                return Task.FromResult<IEnumerable<INotificationMethodProvider>>(methods
                    // Priority matters to honor user preferences.
                    .OrderBy(provider => selectedMethods.IndexOf(provider.Method))
                    .ThenBy(provider => provider.Name.ToString())
                    .ToList());
            }

            return Task.FromResult<IEnumerable<INotificationMethodProvider>>(methods.OrderBy(provider => provider.Name.ToString()).ToList());
        }

        return Task.FromResult(_notificationMethodProviders);
    }
}
