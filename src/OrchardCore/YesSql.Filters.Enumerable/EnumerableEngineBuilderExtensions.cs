using System;
using YesSql.Filters.Abstractions.Builders;
using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

public static class EnumerableEngineBuilderExtensions
{
    /// <summary>
    /// Adds a term where the name must be specified to an <see cref="EnumerableEngineBuilder{T}"/>
    /// </summary>
    public static EnumerableEngineBuilder<T> WithNamedTerm<T>(
        this EnumerableEngineBuilder<T> builder,
        string name,
        Action<NamedTermEngineBuilder<T, EnumerableTermOption<T>>> action
    )
        where T : class
    {
        var parserBuilder = new NamedTermEngineBuilder<T, EnumerableTermOption<T>>(name);
        action(parserBuilder);

        builder.SetTermParser(parserBuilder);
        return builder;
    }

    /// <summary>
    /// Adds a term where the name is optional to an <see cref="EnumerableEngineBuilder{T}"/>
    /// </summary>
    public static EnumerableEngineBuilder<T> WithDefaultTerm<T>(
        this EnumerableEngineBuilder<T> builder,
        string name,
        Action<DefaultTermEngineBuilder<T, EnumerableTermOption<T>>> action
    )
        where T : class
    {
        var parserBuilder = new DefaultTermEngineBuilder<T, EnumerableTermOption<T>>(name);
        action(parserBuilder);

        builder.SetTermParser(parserBuilder);
        return builder;
    }
}
