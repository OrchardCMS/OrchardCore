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

    /// <summary>
    /// Attempts to send the specified notification message by using all notification methods available for the recipient.
    /// </summary>
    /// <param name="notify">The recipient or notifiable object.</param>
    /// <param name="message">The notification message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="NotificationSendResult"/> that describes how many notification methods succeeded or failed.</returns>
    public async Task<NotificationSendResult> SendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default)
    {
        var notificationContext = new NotificationContext(message, notify);

        var notification = await CreateNotificationAsync(notificationContext, cancellationToken);

        await _notificationEvents.InvokeAsync((handler, context, token) => handler.SendingAsync(context, token), notificationContext, cancellationToken, _logger);

        var sendResult = new NotificationSendResult();

        var providers = await _notificationMethodProviderAccessor.GetProvidersAsync(notify);

        foreach (var provider in providers)
        {
            await _notificationEvents.InvokeAsync((handler, service, context, token) => handler.SendingAsync(service, context, token), provider, notificationContext, cancellationToken, _logger);

            var result = await provider.SendAsync(notify, message, cancellationToken);

            if (result.Succeeded)
            {
                sendResult.SuccessfulCount++;

                await _notificationEvents.InvokeAsync((handler, service, context, token) => handler.SentAsync(service, context, token), provider, notificationContext, cancellationToken, _logger);
            }
            else
            {
                sendResult.FailedCount++;

                foreach (var error in result.Errors)
                {
                    sendResult.AddError(error.Message, error.Key);
                }

                await _notificationEvents.InvokeAsync((handler, service, context, token) => handler.FailedAsync(service, context, token), provider, notificationContext, cancellationToken, _logger);
            }
        }

        await _notificationEvents.InvokeAsync((handler, context, token) => handler.SentAsync(context, token), notificationContext, cancellationToken, _logger);

        return sendResult;
    }

    private async Task<Notification> CreateNotificationAsync(NotificationContext context, CancellationToken cancellationToken)
    {
        var notification = new Notification()
        {
            NotificationId = IdGenerator.GenerateId(),
            CreatedUtc = _clock.UtcNow,
            Subject = context.NotificationMessage.Subject,
            Summary = context.NotificationMessage.Summary,
        };

        context.Notification = notification;

        await _notificationEvents.InvokeAsync((handler, notificationContext, token) => handler.CreatingAsync(notificationContext, token), context, cancellationToken, _logger);
        await _session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection, cancellationToken: cancellationToken);
        await _notificationEvents.InvokeAsync((handler, notificationContext, token) => handler.CreatedAsync(notificationContext, token), context, cancellationToken, _logger);

        return notification;
    }
}
