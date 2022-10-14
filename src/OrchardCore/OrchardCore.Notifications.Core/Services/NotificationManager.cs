using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Entities;
using OrchardCore.Notifications.Models;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Notifications.Services;

public class NotificationManager : INotificationManager
{
    private readonly IEnumerable<INotificationMethodProvider> _notificationMethodProviders;
    private readonly ILogger _logger;

    public NotificationManager(IEnumerable<INotificationMethodProvider> notificationMethodProviders,
        ILogger<NotificationManager> logger)
    {
        _notificationMethodProviders = notificationMethodProviders;
        _logger = logger;
    }

    public async Task<int> SendAsync(IUser user, INotificationMessage message)
    {
        var totalSent = 0;

        if (user is User su)
        {
            var notificationPart = su.As<UserNotificationPart>();

            // Attempt to send the notification top to bottom as the priority matters in this case.
            var selectedMethods = (notificationPart?.Methods) ?? Array.Empty<string>();

            foreach (var selectedMethod in selectedMethods)
            {
                var sender = _notificationMethodProviders.FirstOrDefault(s => String.Equals(s.Method, selectedMethod, StringComparison.OrdinalIgnoreCase));

                if (sender == null)
                {
                    _logger.LogWarning("No {notificationMethod} to handle method {selectedMethod}", nameof(INotificationMethodProvider), selectedMethod);

                    continue;
                }

                if (await sender.TrySendAsync(user, message))
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
