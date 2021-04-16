using System.Collections.Generic;
using System.Linq;
using OrchardCore.Filters.Abstractions.Builders;
using OrchardCore.Filters.Query.Services;

namespace OrchardCore.Filters.Query
{
    public class QueryEngineBuilder<T> where T : class
    {
        private Dictionary<string, TermEngineBuilder<T, QueryTermOption<T>>> _termBuilders = new Dictionary<string, TermEngineBuilder<T, QueryTermOption<T>>>();

        public QueryEngineBuilder<T> SetTermParser(TermEngineBuilder<T, QueryTermOption<T>> builder)
        {
            _termBuilders[builder.Name] = builder;

            return this;
        }

        public IQueryParser<T> Build()
        {
            var builders = _termBuilders.Values.Select(x => x.Build());

            var parsers = builders.Select(x => x.Parser).ToArray();
            var termOptions = builders.Select(x => x.TermOption).ToDictionary(k => k.Name, v => v);

            return new QueryParser<T>(parsers, termOptions);
        }
    }
}
