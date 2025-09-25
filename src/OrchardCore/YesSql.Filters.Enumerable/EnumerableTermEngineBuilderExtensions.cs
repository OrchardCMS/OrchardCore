using YesSql.Filters.Abstractions.Builders;
using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

public static class QueryTermFilterBuilderExtensions
{
    /// <summary>
    /// Adds a single condition to a <see cref="TermEngineBuilder{T, TTermOption}"/>.
    /// <param name="builder"></param>.
    /// <param name="matchQuery">The predicate to apply when the term is parsed.</param>
    /// </summary>
    public static EnumerableUnaryEngineBuilder<T> OneCondition<T>(
        this TermEngineBuilder<T, EnumerableTermOption<T>> builder,
        Func<string, IEnumerable<T>, IEnumerable<T>> matchQuery
    )
        where T : class
    {
        ValueTask<IEnumerable<T>> valueQuery(
            string q,
            IEnumerable<T> val,
            EnumerableExecutionContext<T> ctx
        ) => new(matchQuery(q, val));

        return builder.OneCondition(valueQuery);
    }

    /// <summary>
    /// Adds a single condition to a <see cref="TermEngineBuilder{T, TTermOption}"/>.
    /// <param name="builder"></param>
    /// <param name="matchQuery">An async predicate to apply when the term is parsed.</param>
    /// </summary>
    public static EnumerableUnaryEngineBuilder<T> OneCondition<T>(
        this TermEngineBuilder<T, EnumerableTermOption<T>> builder,
        Func<
            string,
            IEnumerable<T>,
            EnumerableExecutionContext<T>,
            ValueTask<IEnumerable<T>>
        > matchQuery
    )
        where T : class
    {
        var operatorBuilder = new EnumerableUnaryEngineBuilder<T>(builder.Name, matchQuery);
        builder.SetOperator(operatorBuilder);

        return operatorBuilder;
    }

    /// <summary>
    /// Adds a condition which supports many operations to a <see cref="TermEngineBuilder{T, TTermOption}"/>
    /// <param name="builder"></param>
    /// <param name="matchQuery">The predicate to apply when the term is parsed with an AND or OR operator.</param>
    /// <param name="notMatchQuery">The predicate to apply when the term is parsed with a NOT operator.</param>
    /// </summary>
    public static EnumerableBooleanEngineBuilder<T> ManyCondition<T>(
        this TermEngineBuilder<T, EnumerableTermOption<T>> builder,
        Func<string, IEnumerable<T>, IEnumerable<T>> matchQuery,
        Func<string, IEnumerable<T>, IEnumerable<T>> notMatchQuery
    )
        where T : class
    {
        ValueTask<IEnumerable<T>> valueMatch(
            string q,
            IEnumerable<T> val,
            EnumerableExecutionContext<T> ctx
        ) => new(matchQuery(q, val));
        ValueTask<IEnumerable<T>> valueNotMatch(
            string q,
            IEnumerable<T> val,
            EnumerableExecutionContext<T> ctx
        ) => new(notMatchQuery(q, val));

        return builder.ManyCondition(valueMatch, valueNotMatch);
    }

    /// <summary>
    /// Adds a condition which supports many operations to a <see cref="TermEngineBuilder{T, TTermOption}"/>
    /// <param name="builder"></param>.
    /// <param name="matchQuery">The predicate to apply when the term is parsed with an AND or OR operator.</param>
    /// <param name="notMatchQuery">The predicate to apply when the term is parsed with a NOT operator.</param>
    /// </summary>
    public static EnumerableBooleanEngineBuilder<T> ManyCondition<T>(
        this TermEngineBuilder<T, EnumerableTermOption<T>> builder,
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
        where T : class
    {
        var operatorBuilder = new EnumerableBooleanEngineBuilder<T>(
            builder.Name,
            matchQuery,
            notMatchQuery
        );
        builder.SetOperator(operatorBuilder);

        return operatorBuilder;
    }
}
