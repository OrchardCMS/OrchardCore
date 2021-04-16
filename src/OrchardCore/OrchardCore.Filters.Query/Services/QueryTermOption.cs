using System;
using System.Threading.Tasks;
using OrchardCore.Filters.Abstractions.Services;
using YesSql;

namespace OrchardCore.Filters.Query.Services
{
    public class QueryTermOption<T> : TermOption where T : class
    {
        public QueryTermOption(string name, Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> matchPredicate) : base(name)
        {
            MatchPredicate = matchPredicate;
        }

        public QueryTermOption(string name, Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> matchPredicate, Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> notMatchPredicate)
            : base(name)
        {
            MatchPredicate = matchPredicate;
            NotMatchPredicate = notMatchPredicate;
        }
        public Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> MatchPredicate { get; }
        public Func<string, IQuery<T>, QueryExecutionContext<T>, ValueTask<IQuery<T>>> NotMatchPredicate { get; }
    }
}
