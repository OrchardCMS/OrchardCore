using System.Security.Claims;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Notifications.Services;

public sealed class NotificationVisibilityEvaluator : INotificationVisibilityEvaluator
{
    private readonly INotificationVisibilityHandler[] _handlers;
    private readonly ILogger _logger;

    // The decisions are kept per user and notification, as a request can show the same notification more than once.
    private readonly Dictionary<(string UserId, string NotificationId), NotificationVisibility> _decisions = [];

    public NotificationVisibilityEvaluator(
        IEnumerable<INotificationVisibilityHandler> handlers,
        ILogger<NotificationVisibilityEvaluator> logger)
    {
        _handlers = handlers.ToArray();
        _logger = logger;
    }

    public bool HasHandlers => _handlers.Length > 0;

    public async Task<NotificationVisibility> EvaluateAsync(Notification notification, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        if (_handlers.Length == 0)
        {
            return NotificationVisibility.Visible;
        }

        await EvaluateMissingAsync([notification], user, cancellationToken);

        return _decisions[GetKey(notification, user)];
    }

    public async Task<List<Notification>> FilterAsync(IEnumerable<Notification> notifications, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        var list = notifications.ToList();

        if (_handlers.Length == 0)
        {
            return list;
        }

        await EvaluateMissingAsync(list, user, cancellationToken);

        return list.FindAll(notification => _decisions[GetKey(notification, user)] != NotificationVisibility.Hidden);
    }

    private async Task EvaluateMissingAsync(IEnumerable<Notification> notifications, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var contexts = notifications
            .Where(notification => !_decisions.ContainsKey(GetKey(notification, user)))
            .DistinctBy(notification => notification.NotificationId)
            .Select(notification => new NotificationVisibilityContext(notification, user))
            .ToList();

        if (contexts.Count == 0)
        {
            return;
        }

        foreach (var handler in _handlers)
        {
            try
            {
                await handler.EvaluateAsync(contexts, cancellationToken);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                if (contexts.Count == 1)
                {
                    // A notification whose access could not be checked must not be shown.
                    _logger.LogError(ex, "The handler '{Handler}' failed to evaluate a notification, which is hidden.", handler.GetType().FullName);

                    contexts[0].Hide();

                    continue;
                }

                // One notification can make the whole batch fail, for example one of a type the handler doesn't expect:
                // each notification is evaluated on its own, so that only those that fail are hidden.
                _logger.LogWarning(ex, "The handler '{Handler}' failed to evaluate {Count} notifications, which are evaluated one by one.", handler.GetType().FullName, contexts.Count);

                await EvaluateOneByOneAsync(handler, contexts, cancellationToken);
            }
        }

        foreach (var context in contexts)
        {
            _decisions[GetKey(context.Notification, user)] = context.Visibility;
        }
    }

    private async Task EvaluateOneByOneAsync(INotificationVisibilityHandler handler, List<NotificationVisibilityContext> contexts, CancellationToken cancellationToken)
    {
        Exception lastException = null;
        var hidden = 0;

        foreach (var context in contexts)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await handler.EvaluateAsync([context], cancellationToken);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                // A notification whose access could not be checked must not be shown.
                context.Hide();
                lastException = ex;
                hidden++;
            }
        }

        if (hidden > 0)
        {
            _logger.LogError(lastException, "The handler '{Handler}' failed to evaluate {Count} of {Total} notifications, which are hidden.", handler.GetType().FullName, hidden, contexts.Count);
        }
    }

    private static (string UserId, string NotificationId) GetKey(Notification notification, ClaimsPrincipal user)
        => (user?.FindFirstValue(ClaimTypes.NameIdentifier), notification.NotificationId);
}
