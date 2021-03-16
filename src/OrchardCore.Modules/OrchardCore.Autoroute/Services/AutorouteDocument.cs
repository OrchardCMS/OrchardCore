using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteDocument : Document
    {
        public Dictionary<string, AutorouteEntry> Paths { get; set; } = new Dictionary<string, AutorouteEntry>();
        public Dictionary<string, AutorouteEntry> ContentItemIds { get; set; } = new Dictionary<string, AutorouteEntry>(StringComparer.OrdinalIgnoreCase);
    }
}
