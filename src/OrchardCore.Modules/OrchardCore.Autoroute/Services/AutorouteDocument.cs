using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteDocument : Document
    {
        public Dictionary<string, string> Paths { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> ContentItemIds { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
