using System.Collections.Generic;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEventsDocument : Document
    {
        public List<AutorouteEvent> List { get; set; } = new List<AutorouteEvent>();
    }

    public class AutorouteEvent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<AutorouteEntry> Entries { get; set; }
    }
}
