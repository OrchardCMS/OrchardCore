using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

public static class EnumerableFilterResultExtensions
{
    public static ValueTask<IEnumerable<T>> ExecuteAsync<T>(
        this EnumerableFilterResult<T> result,
        IEnumerable<T> query
    )
        where T : class => result.ExecuteAsync(new EnumerableExecutionContext<T>(query));
}
