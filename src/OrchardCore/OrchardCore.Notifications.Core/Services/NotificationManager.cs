using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using OrchardCore.Users;
using YesSql;

namespace OrchardCore.Notifications.Services;

public class NotificationManager : INotificationManager
{
    private readonly INotificationMethodProviderAccessor _notificationMethodProviderAccessor;
    private readonly IEnumerable<INotificationEvents> _notificationEvents;
    private readonly ILogger<NotificationManager> _logger;
    private readonly ISession _session;
    private readonly IClock _clock;

    public NotificationManager(INotificationMethodProviderAccessor notificationMethodProviderAccessor,
        IEnumerable<INotificationEvents> notificationEvents,
        ILogger<NotificationManager> logger,
        ISession session,
        IClock clock)
    {
        _notificationMethodProviderAccessor = notificationMethodProviderAccessor;
        _notificationEvents = notificationEvents;
        _logger = logger;
        _session = session;
        _clock = clock;
    }

    public async Task<int> SendAsync(IUser user, INotificationMessage message)
    {
        var providers = await _notificationMethodProviderAccessor.GetProvidersAsync(user);

        var notification = await CreateNotificationAsync(message, user);

        var notificationContext = new NotificationContext()
        {
            NotificationMessage = message,
            User = user,
            Notification = notification,
        };

        await _notificationEvents.InvokeAsync((handler, context) => handler.SendingAsync(context), notificationContext, _logger);

        var totalSent = 0;

        foreach (var provider in providers)
        {
            await _notificationEvents.InvokeAsync((handler, service, context) => handler.SendingAsync(service, notificationContext), provider, notificationContext, _logger);

            if (await provider.TrySendAsync(user, message))
            {
                await _notificationEvents.InvokeAsync((handler, service, context) => handler.SentAsync(service, notificationContext), provider, notificationContext, _logger);

                totalSent++;
            }
            else
            {
                await _notificationEvents.InvokeAsync((handler, service, context) => handler.FailedAsync(service, notificationContext), provider, notificationContext, _logger);
            }
        }

        if (totalSent > 0)
        {
            await _notificationEvents.InvokeAsync((handler, context) => handler.SentAsync(context), notificationContext, _logger);
        }

        return totalSent;
    }

    private async Task<Notification> CreateNotificationAsync(INotificationMessage message, IUser user)
    {
        var notification = new Notification()
        {
            NotificationId = IdGenerator.GenerateId(),
            CreatedUtc = _clock.UtcNow,
            UserId = user.UserName,
            Subject = message.Subject,
            Body = message.Body,
        };

        var context = new NotificationContext()
        {
            Notification = notification,
            NotificationMessage = message,
            User = user,
        };

        await _notificationEvents.InvokeAsync((handler, context) => handler.CreatingAsync(context), context, _logger);
        _session.Save(notification, collection: NotificationConstants.NotificationCollection);
        await _notificationEvents.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger);

        return notification;
    }
}
