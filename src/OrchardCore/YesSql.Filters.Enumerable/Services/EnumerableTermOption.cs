using YesSql.Filters.Abstractions.Services;

namespace YesSql.Filters.Enumerable.Services;

public class EnumerableTermOption<T> : TermOption
    where T : class
{
    public EnumerableTermOption(
        string name,
        Func<
            string,
            IEnumerable<T>,
            EnumerableExecutionContext<T>,
            ValueTask<IEnumerable<T>>
        > matchPredicate
    )
        : base(name)
    {
        MatchPredicate = matchPredicate;
    }

    public EnumerableTermOption(
        string name,
        Func<
            string,
            IEnumerable<T>,
            EnumerableExecutionContext<T>,
            ValueTask<IEnumerable<T>>
        > matchPredicate,
        Func<
            string,
            IEnumerable<T>,
            EnumerableExecutionContext<T>,
            ValueTask<IEnumerable<T>>
        > notMatchPredicate
    )
        : base(name)
    {
        MatchPredicate = matchPredicate;
        NotMatchPredicate = notMatchPredicate;
    }

    public Func<
        string,
        IEnumerable<T>,
        EnumerableExecutionContext<T>,
        ValueTask<IEnumerable<T>>
    > MatchPredicate { get; }
    public Func<
        string,
        IEnumerable<T>,
        EnumerableExecutionContext<T>,
        ValueTask<IEnumerable<T>>
    > NotMatchPredicate { get; }
}
