using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.BackgroundJobs.Models
{
    public class BackgroundJobDocument : Document
    {
        public Dictionary<string, BackgroundJobEntry> Entries = new(StringComparer.OrdinalIgnoreCase);
    }
}
