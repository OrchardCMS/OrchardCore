using System;
using System.Collections.Immutable;

namespace OrchardCore.Queries.Services
{
    public class QueriesDocument
    {
        public int Id { get; set; } // An identifier so that updates don't create new documents
        public IImmutableDictionary<string, Query> Queries { get; set; } = ImmutableDictionary.Create<string, Query>(StringComparer.OrdinalIgnoreCase);
    }
}
