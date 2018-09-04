using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Abstractions.Services;

namespace OrchardCore.WebHooks.Services
{
    /// <summary>
    /// Provides an implementation of <see cref="IWebHookEventManager"/> which provides the set of 
    /// registered <see cref="IWebHookEventProvider"/> instances.
    /// </summary>
    public class WebHookEventManagaer : IWebHookEventManager
    {
        private readonly IEnumerable<IWebHookEventProvider> _providers;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebHookEventManagaer"/> class with the 
        /// given <paramref name="providers"/>.
        /// </summary>
        public WebHookEventManagaer(IEnumerable<IWebHookEventProvider> providers)
        {
            _providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        public Task<List<WebHookEvent>> GetAllWebHookEventsAsync()
        {
            var events = new List<WebHookEvent>();
            foreach (var eventProvider in _providers)
            {
                events.AddRange(eventProvider.GetEvents());
            }

            return Task.FromResult(events);
        }

        public Task<HashSet<string>> NormalizeEventsAsync(IEnumerable<string> submittedEvents)
        {
            var events = new HashSet<string>();
            foreach (var eventProvider in _providers)
            {
                events.UnionWith(eventProvider.NormalizeEvents(submittedEvents));
            }

            return Task.FromResult(events);
        }
    }
}
