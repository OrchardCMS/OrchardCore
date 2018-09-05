using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.WebHooks.Abstractions.Models;
using OrchardCore.WebHooks.Abstractions.Services;

namespace OrchardCore.WebHooks.Services.Events
{
    /// <summary>
    /// Defines a default wildcard <see cref="WebHookEvent"/> which matches all events.
    /// </summary>
    public class WildcardWebHookEvent : IWebHookEventProvider
    {
        public static readonly WebHookEvent WildcardEvent = new WebHookEvent("*", category: "Wildcard");

        public IEnumerable<WebHookEvent> GetEvents()
        {
            return new[] { WildcardEvent };
        }

        public IEnumerable<string> NormalizeEvents(IEnumerable<string> submittedEvents)
        {
            if(submittedEvents == null) throw new ArgumentNullException(nameof(submittedEvents));

            // If there are no submitted events or the wildcard event exists then add just
            // our wildcard event since any additional subscribed events are redundant.
            var events = submittedEvents.ToList();
            if (!events.Any() || events.Contains(WildcardEvent.Name))
            {
                return new HashSet<string>(new[] {WildcardEvent.Name});
            }

            return new HashSet<string>();
        }
    }
}