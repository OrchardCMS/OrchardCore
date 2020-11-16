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

        public void AddEvent(string name, IEnumerable<AutorouteEntry> entries)
        {
            Events.Add(new AutorouteEvent()
            {
                Name = name,
                Id = Identifier,
                Entries = new List<AutorouteEntry>(entries)
            });

            // Limit the events list length.
            if (Events.Count > MaxEventsCount)
            {
                Events = Events.Skip(Events.Count - MaxEventsCount).ToList();
            }
        }

        public void AddEntriesEvent(IEnumerable<AutorouteEntry> entries) => AddEvent(AutorouteEvent.AddEntries, entries);
        public void RemoveEntriesEvent(IEnumerable<AutorouteEntry> entries) => AddEvent(AutorouteEvent.RemoveEntries, entries);

        public bool TryGetNewEvents(string lastEventId, out IEnumerable<AutorouteEvent> events)
        {
            var index = Events.FindLastIndex(x => x.Id == lastEventId);
            if (index != -1)
            {
                if (Events.Count == index + 1)
                {
                    // Return no new events, we are up to date.
                    events = Enumerable.Empty<AutorouteEvent>();
                    return true;
                }

                // Return the events that are not yet processed.
                events = Events.Skip(index + 1);
                return true;
            }

            if (Events.Count >= MaxEventsCount)
            {
                // The last event was not found and the max count was reached,
                // so we may have missed some events.
                events = null;
                return false;
            }

            // Otherwise return the full list.
            events = Events;
            return true;
        }
    }

    public class AutorouteEvent
    {
        public const string AddEntries = nameof(AddEntries);
        public const string RemoveEntries = nameof(RemoveEntries);

        public string Id { get; set; }
        public string Name { get; set; }
        public List<AutorouteEntry> Entries { get; set; }
    }
}
