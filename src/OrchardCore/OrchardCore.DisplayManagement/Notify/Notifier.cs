using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.DisplayManagement.Notify;

public class Notifier : INotifier
{
    private readonly List<NotifyEntry> _entries = [];
    private readonly ILogger _logger;

    public Notifier(ILogger<Notifier> logger)
    {
        _logger = logger;
    }

    public ValueTask AddAsync(NotifyType type, LocalizedHtmlString message)
    {
        _logger.LogInformation("Notification '{NotificationType}' with message '{NotificationMessage}'", type, message);

        _entries.Add(new NotifyEntry { Type = type, Message = message });

        return ValueTask.CompletedTask;
    }

    public IList<NotifyEntry> List()
        => _entries;
}
