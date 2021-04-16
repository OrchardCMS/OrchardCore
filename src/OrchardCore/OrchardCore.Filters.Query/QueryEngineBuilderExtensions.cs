using System;
using OrchardCore.Filters.Abstractions.Builders;
using OrchardCore.Filters.Query.Services;

namespace OrchardCore.Filters.Query
{
    public static class QueryEngineBuilderExtensions
    {
        public static QueryEngineBuilder<T> WithNamedTerm<T>(this QueryEngineBuilder<T> builder, string name, Action<NamedTermEngineBuilder<T, QueryTermOption<T>>> action) where T : class
        {
            var parserBuilder = new NamedTermEngineBuilder<T, QueryTermOption<T>>(name);
            action(parserBuilder);

            builder.SetTermParser(parserBuilder);
            return builder;
        }

        public static QueryEngineBuilder<T> WithDefaultTerm<T>(this QueryEngineBuilder<T> builder, string name, Action<DefaultTermEngineBuilder<T, QueryTermOption<T>>> action) where T : class
        {
            var parserBuilder = new DefaultTermEngineBuilder<T, QueryTermOption<T>>(name);
            action(parserBuilder);

            builder.SetTermParser(parserBuilder);
            return builder;
        }
    }
}
