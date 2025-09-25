using YesSql.Filters.Abstractions.Builders;
using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

public class EnumerableBooleanEngineBuilder<T> : BooleanEngineBuilder<T, EnumerableTermOption<T>>
    where T : class
{
    public EnumerableBooleanEngineBuilder(
        string name,
        Func<
            string,
            IEnumerable<T>,
            EnumerableExecutionContext<T>,
            ValueTask<IEnumerable<T>>
        > matchQuery,
        Func<
            string,
            IEnumerable<T>,
            EnumerableExecutionContext<T>,
            ValueTask<IEnumerable<T>>
        > notMatchQuery
    )
    {
        _termOption = new EnumerableTermOption<T>(name, matchQuery, notMatchQuery);
    }
}
