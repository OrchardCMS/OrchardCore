using YesSql.Filters.Abstractions.Builders;
using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

/// <summary>
/// Builds a <see cref="EnumerableEngineBuilder{T}"/> for an <see cref="IEnumerable{T}"/>.
/// </summary>
public class EnumerableEngineBuilder<T>
    where T : class
{
    private readonly Dictionary<
        string,
        TermEngineBuilder<T, EnumerableTermOption<T>>
    > _termBuilders = new Dictionary<string, TermEngineBuilder<T, EnumerableTermOption<T>>>();

    public EnumerableEngineBuilder<T> SetTermParser(
        TermEngineBuilder<T, EnumerableTermOption<T>> builder
    )
    {
        _termBuilders[builder.Name] = builder;

        return this;
    }

    public IEnumerableParser<T> Build()
    {
        var builders = _termBuilders.Values.Select(x => x.Build());

        var parsers = builders.Select(x => x.Parser).ToArray();
        var termOptions = builders
            .Select(x => x.TermOption)
            .ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

        return new EnumerableParser<T>(parsers, termOptions);
    }
}
