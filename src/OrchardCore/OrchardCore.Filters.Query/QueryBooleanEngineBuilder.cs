using System;
using System.Threading.Tasks;
using YesSql;
using OrchardCore.Filters.Abstractions.Builders;
using OrchardCore.Filters.Query.Services;

namespace OrchardCore.Filters.Query
{
    public class QueryBooleanEngineBuilder<T> : BooleanEngineBuilder<T, QueryTermOption<T>> where T : class
    {
        public QueryBooleanEngineBuilder(
            string name,
            Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> matchQuery,
            Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> notMatchQuery)
        {
            _termOption = new QueryTermOption<T>(name, matchQuery, notMatchQuery);
        }
    }
}
