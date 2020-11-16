using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEventsDocument : Document
    {
        public const int MaxEventsCount = 100;

        public List<AutorouteEvent> Events { get; set; } = new List<AutorouteEvent>();

        public void AddEvent(AutorouteEvent @event)
        {
            Events.Add(@event);

            // Limit the events list length.
            if (Events.Count > MaxEventsCount)
            {
                Events = Events.Skip(Events.Count - MaxEventsCount).ToList();
            }
        }

        public IEnumerable<AutorouteEvent> GetNewEvents(string lastEventId)
        {
            var index = Events.FindLastIndex(x => x.Id == lastEventId);
            if (index != -1)
            {
                if (Events.Count == index + 1)
                {
                    // Return no new events, we are up to date.
                    return Enumerable.Empty<AutorouteEvent>();
                }

                // Return the events that are not yet processed.
                return Events.Skip(index + 1);
            }

            if (Events.Count >= MaxEventsCount)
            {
                // The last event was not found and the max count was reached,
                // so we may have missed some events.
                return null;
            }

            // Otherwise return the full list.
            return Events;
        }
    }

    public class AutorouteEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<AutorouteEntry> Entries { get; set; }
    }
}
