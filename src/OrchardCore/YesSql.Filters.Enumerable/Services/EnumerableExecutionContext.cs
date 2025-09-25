using YesSql.Filters.Abstractions.Services;

namespace YesSql.Filters.Enumerable.Services;

public class EnumerableExecutionContext<T> : FilterExecutionContext<IEnumerable<T>>
    where T : class
{
    public EnumerableExecutionContext(IEnumerable<T> query)
        : base(query) { }

    public EnumerableTermOption<T> CurrentTermOption { get; set; }
}
