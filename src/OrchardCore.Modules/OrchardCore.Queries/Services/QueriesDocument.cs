using System;
using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.Queries.Services
{
    public class QueriesDocument : Document
    {
        public Dictionary<string, Query> Queries { get; set; } = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);
    }
}
