using System.Security.Claims;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;

namespace OrchardCore.Notifications.Services;

public sealed class NotificationVisibilityEvaluator : INotificationVisibilityEvaluator
{
    // The failed calls allowed after a batch failed. Splitting the batch isolates one notification among 256 within
    // this limit, and it bounds the calls made when the handler fails on all of them, for example when a service is down.
    private const int MaxFailedRetries = 8;

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

    public bool HasFailures { get; private set; }

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
            await EvaluateWithHandlerAsync(handler, contexts, cancellationToken);
        }

        foreach (var context in contexts)
        {
            _decisions[GetKey(context.Notification, user)] = context.Visibility;
        }
    }

    private async Task EvaluateWithHandlerAsync(INotificationVisibilityHandler handler, List<NotificationVisibilityContext> contexts, CancellationToken cancellationToken)
    {
        var batches = new Stack<List<NotificationVisibilityContext>>();
        batches.Push(contexts);

        List<Exception> failures = null;
        List<string> hiddenIds = null;

        while (batches.TryPop(out var batch))
        {
            if (failures != null)
            {
                batch = batch.FindAll(context => context.Visibility != NotificationVisibility.Hidden);

                if (failures.Count > MaxFailedRetries)
                {
                    batch.ForEach(Hide);

                    continue;
                }

                if (batch.Count == 0)
                {
                    continue;
                }
            }

            try
            {
                await handler.EvaluateAsync(batch, cancellationToken);
            }
            catch (Exception ex) when (!ex.IsFatal() && !cancellationToken.IsCancellationRequested)
            {
                (failures ??= []).Add(ex);

                if (batch.Count == 1)
                {
                    Hide(batch[0]);

                    continue;
                }

                // One notification can make the whole batch fail, for example one of a type the handler doesn't expect:
                // the batch is split, so that only the notifications that fail are hidden.
                var middle = batch.Count / 2;

                batches.Push(batch.GetRange(middle, batch.Count - middle));
                batches.Push(batch.GetRange(0, middle));
            }
        }

        if (failures == null)
        {
            return;
        }

        var exception = failures.Count == 1 ? failures[0] : new AggregateException(failures);

        if (hiddenIds == null)
        {
            _logger.LogWarning(exception, "The handler '{Handler}' failed to evaluate {Count} notifications at once, but none had to be hidden.", handler.GetType().FullName, contexts.Count);

            return;
        }

        HasFailures = true;

        _logger.LogError(exception, "The handler '{Handler}' failed to evaluate the notifications {NotificationIds}, which are hidden.", handler.GetType().FullName, string.Join(", ", hiddenIds));

        // A notification whose access could not be checked must not be shown.
        void Hide(NotificationVisibilityContext context)
        {
            context.Hide();
            (hiddenIds ??= []).Add(context.Notification.NotificationId);
        }
    }

    private static (string UserId, string NotificationId) GetKey(Notification notification, ClaimsPrincipal user)
        => (user?.FindFirstValue(ClaimTypes.NameIdentifier), notification.NotificationId);
}
