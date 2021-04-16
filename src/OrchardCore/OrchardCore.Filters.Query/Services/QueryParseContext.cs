using System.Collections.Generic;
using Parlot;
using Parlot.Fluent;

namespace OrchardCore.Filters.Query.Services
{
    public class QueryParseContext<T> : ParseContext where T : class
    {
        public QueryParseContext(IReadOnlyDictionary<string, QueryTermOption<T>> termOptions, Scanner scanner, bool useNewLines = false) : base(scanner, useNewLines)
        {
            TermOptions = termOptions;
        }

        public IReadOnlyDictionary<string, QueryTermOption<T>> TermOptions { get; }
    }
}
