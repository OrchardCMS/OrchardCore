using System;
using System.Collections.Generic;

namespace OrchardCore.Queries.Services
{
    public class QueriesDocument
    {
        public int Id { get; set; }
        public Dictionary<string, Query> Queries { get; set; } = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);
    }
}
