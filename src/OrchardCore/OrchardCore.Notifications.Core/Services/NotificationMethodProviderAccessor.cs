using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class NotificationMethodProviderAccessor : INotificationMethodProviderAccessor
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;
    private readonly NotificationTypeOptions _notificationTypeOptions;

    public NotificationMethodProviderAccessor(
        IEnumerable<INotificationMethodProvider> notificationMethodProviders,
        IOptions<NotificationTypeOptions> notificationTypeOptions)
    {
        _notificationMethodProviders = notificationMethodProviders;
        _notificationTypeOptions = notificationTypeOptions.Value;
    }

    public Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify)
        => GetProvidersAsync(notify, type: null);

    public Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify, INotificationMessage message)
    {
        NotificationTypeDescriptor type = null;

        if (!string.IsNullOrEmpty(message?.Type))
        {
            _notificationTypeOptions.Types.TryGetValue(message.Type, out type);
        }

        return GetProvidersAsync(notify, type);
    }

    private Task<IEnumerable<INotificationMethodProvider>> GetProvidersAsync(object notify, NotificationTypeDescriptor type)
    {
        var user = notify as User;

        if (user != null)
        {
            List<string> selectedMethods = [];
            string[] optout = [];

            user.TryGet<UserNotificationPreferencesPart>(out var notificationPart);

            if (notificationPart != null)
            {
                selectedMethods = notificationPart.Methods?.ToList() ?? [];
                optout = notificationPart.Optout ?? [];
            }

            var methods = _notificationMethodProviders.Where(provider => !optout.Contains(provider.Method));

            if (type != null)
            {
                methods = methods.Where(provider => NotificationTypePreferences.IsMethodEnabledForType(notificationPart, type, provider.Method));
            }

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

        if (type != null)
        {
            return Task.FromResult<IEnumerable<INotificationMethodProvider>>(_notificationMethodProviders
                .Where(provider => !type.DisabledMethods.Contains(provider.Method))
                .ToList());
        }

        return Task.FromResult(_notificationMethodProviders);
    }
}
