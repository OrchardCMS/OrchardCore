using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.DisplayManagement.Notify
{
    public class Notifier : INotifier
    {
        private readonly IList<NotifyEntry> _entries;
        private readonly ILogger _logger;

        public Notifier(ILogger<Notifier> logger)
        {
            _entries = new List<NotifyEntry>();
            _logger = logger;
        }

        public void Add(NotifyType type, LocalizedHtmlString message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Notification '{NotificationType}' with message '{NotificationMessage}'", type, message);
            }

            _entries.Add(new NotifyEntry { Type = type, Message = message });
        }

        public IList<NotifyEntry> List()
        {
            return _entries;
        }
    }
}
