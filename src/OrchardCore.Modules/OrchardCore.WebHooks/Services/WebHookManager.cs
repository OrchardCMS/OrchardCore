using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Abstractions.Services;
using OrchardCore.WebHooks.Extensions;

namespace OrchardCore.WebHooks.Services
{
    public class WebHookManager : IWebHookManager
    {
        private readonly IWebHookSender _sender;
        private readonly IWebHookStore _store;

        public WebHookManager(IWebHookSender sender, IWebHookStore store, ILogger<WebHookManager> logger)
        {
            _sender = sender;
            _store = store;
            
            Logger = logger;
        }

        public ILogger Logger { get; set; }

        public async Task NotifyAsync(string eventName, JObject defaultPayload, Dictionary<string, object> properties)
        {
            if (eventName == null) throw new ArgumentNullException(nameof(eventName));

            try
            {
                // Get all WebHooks for tenant
                var webHooksList = await _store.GetAllWebHooksAsync();

                // Match any webhooks against the triggered event e.g. *.*, content.created, asset.updated
                var matchedWebHooks = webHooksList.WebHooks.Where(x => x.Enabled && x.MatchesEvent(eventName));

                var context = new WebHookNotificationContext
                {
                    EventName = eventName,
                    DefaultPayload = defaultPayload ?? new JObject(),
                    Properties = properties ?? new Dictionary<string, object>()
                };

                // Send the notification to the matching webhooks
                await _sender.SendNotificationsAsync(matchedWebHooks, context);
            }
            catch (Exception ex)
            {
                var message = $"Failed to notify of webhook event {eventName} due to failure: {ex.Message}";
                Logger.LogError(message, ex);
            }
        }
    }
}
