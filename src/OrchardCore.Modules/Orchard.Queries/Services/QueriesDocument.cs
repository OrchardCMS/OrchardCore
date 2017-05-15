using System;
using System.Collections.Generic;

namespace Orchard.Queries.Services
{
    public class QueriesDocument
    {
        public Dictionary<string, Query> Queries { get; set; } = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);
    }
}
