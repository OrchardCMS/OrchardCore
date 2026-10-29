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

        var key = (user?.FindFirstValue(ClaimTypes.NameIdentifier), notification.NotificationId);

        if (_decisions.TryGetValue(key, out var visibility))
        {
            return visibility;
        }

        var context = new NotificationVisibilityContext(notification, user);

        await _handlers.InvokeAsync((handler, context, token) => handler.EvaluateAsync(context, token), context, cancellationToken, _logger);

        return _decisions[key] = context.Visibility;
    }

    public async Task<List<Notification>> FilterAsync(IEnumerable<Notification> notifications, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notifications);

        var visible = new List<Notification>();

        foreach (var notification in notifications)
        {
            if (await EvaluateAsync(notification, user, cancellationToken) != NotificationVisibility.Hidden)
            {
                visible.Add(notification);
            }
        }

        return visible;
    }
}
