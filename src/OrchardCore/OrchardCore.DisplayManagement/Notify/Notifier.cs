using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.DisplayManagement.Notify
{
    public class Notifier : INotifier
    {
        private readonly List<NotifyEntry> _entries;
        private readonly ILogger _logger;

        public Notifier(ILogger<Notifier> logger)
        {
            _entries = [];
            _logger = logger;
        }

        public void Add(NotifyType type, LocalizedHtmlString message)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask AddAsync(NotifyType type, LocalizedHtmlString message)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Notification '{NotificationType}' with message '{NotificationMessage}'", type, message.ToString());
            }

            _entries.Add(new NotifyEntry { Type = type, Message = message });

            return ValueTask.CompletedTask;
        }

        public IList<NotifyEntry> List()
        {
            return _entries;
        }
    }
}
