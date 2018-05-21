using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Localization;

namespace OrchardCore.DisplayManagement.Notify
{

    public class Notifier : INotifier
    {
        private readonly IList<NotifyEntry> _entries;

        public Notifier(ILogger<Notifier> logger)
        {
            Logger = logger;
            _entries = new List<NotifyEntry>();
        }

        public ILogger Logger { get; set; }

        public void Add(NotifyType type, LocalizedHtmlString message)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("Notification '{NotificationType}' with message '{NotificationMessage}'", type, message);
            }
            
            _entries.Add(new NotifyEntry { Type = type, Message = message });
        }

        public IList<NotifyEntry> List()
        {
            return _entries;
        }
    }
}