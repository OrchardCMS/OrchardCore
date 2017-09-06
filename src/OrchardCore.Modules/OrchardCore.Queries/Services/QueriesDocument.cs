using System;
using System.Collections.Generic;

namespace OrchardCore.Queries.Services
{
    public class QueriesDocument
    {
        public Dictionary<string, Query> Queries { get; set; } = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);
    }
}
