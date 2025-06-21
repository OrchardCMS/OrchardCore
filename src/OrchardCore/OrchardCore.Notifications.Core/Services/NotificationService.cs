using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Notifications.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationMethodProviderAccessor _notificationMethodProviderAccessor;
    private readonly IEnumerable<INotificationEvents> _notificationEvents;
    private readonly ILogger _logger;
    private readonly ISession _session;
    private readonly IClock _clock;

    public NotificationService(INotificationMethodProviderAccessor notificationMethodProviderAccessor,
        IEnumerable<INotificationEvents> notificationEvents,
        ILogger<NotificationService> logger,
        ISession session,
        IClock clock)
    {
        _notificationMethodProviderAccessor = notificationMethodProviderAccessor;
        _notificationEvents = notificationEvents;
        _logger = logger;
        _session = session;
        _clock = clock;
    }

    public async Task<int> SendAsync(object notify, INotificationMessage message)
    {
        var notificationContext = new NotificationContext(message, notify);

        var notification = await CreateNotificationAsync(notificationContext).ConfigureAwait(false);

        await _notificationEvents.InvokeAsync((handler, context) => handler.SendingAsync(context), notificationContext, _logger).ConfigureAwait(false);

        var totalSent = 0;

        var providers = await _notificationMethodProviderAccessor.GetProvidersAsync(notify).ConfigureAwait(false);

        foreach (var provider in providers)
        {
            await _notificationEvents.InvokeAsync((handler, service, context) => handler.SendingAsync(service, notificationContext), provider, notificationContext, _logger).ConfigureAwait(false);

            if (await provider.TrySendAsync(notify, message).ConfigureAwait(false))
            {
                totalSent++;

                await _notificationEvents.InvokeAsync((handler, service, context) => handler.SentAsync(service, notificationContext), provider, notificationContext, _logger).ConfigureAwait(false);
            }
            else
            {
                await _notificationEvents.InvokeAsync((handler, service, context) => handler.FailedAsync(service, notificationContext), provider, notificationContext, _logger).ConfigureAwait(false);
            }
        }

        await _notificationEvents.InvokeAsync((handler, context) => handler.SentAsync(context), notificationContext, _logger).ConfigureAwait(false);

        return totalSent;
    }

    private async Task<Notification> CreateNotificationAsync(NotificationContext context)
    {
        var notification = new Notification()
        {
            NotificationId = IdGenerator.GenerateId(),
            CreatedUtc = _clock.UtcNow,
            Subject = context.NotificationMessage.Subject,
            Summary = context.NotificationMessage.Summary,
        };

        context.Notification = notification;

        await _notificationEvents.InvokeAsync((handler, context) => handler.CreatingAsync(context), context, _logger).ConfigureAwait(false);
        await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection).ConfigureAwait(false);
        await _notificationEvents.InvokeAsync((handler, context) => handler.CreatedAsync(context), context, _logger).ConfigureAwait(false);

        return notification;
    }
}
