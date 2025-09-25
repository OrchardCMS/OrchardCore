using YesSql.Filters.Abstractions.Builders;
using YesSql.Filters.Abstractions.Nodes;
using YesSql.Filters.Abstractions.Services;
using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

public class EnumerableUnaryEngineBuilder<T> : UnaryEngineBuilder<T, EnumerableTermOption<T>>
    where T : class
{
    public EnumerableUnaryEngineBuilder(
        string name,
        Func<string, IEnumerable<T>, EnumerableExecutionContext<T>, ValueTask<IEnumerable<T>>> query
    )
        : base(new EnumerableTermOption<T>(name, query)) { }

    /// <summary>
    /// Adds a mapping function which can be applied to a model.
    /// <typeparam name="TModel">The type of model.</typeparam>
    /// </summary>
    public EnumerableUnaryEngineBuilder<T> MapTo<TModel>(Action<string, TModel> map)
    {
        _termOption.MapTo = map;

        return this;
    }

    /// <summary>
    /// Adds a mapping function where terms can be mapped from a model.
    /// <typeparam name="TModel">The type of model.</typeparam>
    /// <param name="map">Mapping to apply</param>
    /// </summary>
    public EnumerableUnaryEngineBuilder<T> MapFrom<TModel>(Func<TModel, (bool, string)> map)
    {
        static TermNode factory(string name, string value) =>
            new NamedTermNode(name, new UnaryNode(value, OperateNodeQuotes.None));

        return MapFrom(map, factory);
    }

    /// <summary>
    /// Adds a mapping function where terms can be mapped from a model.
    /// <typeparam name="TModel">The type of model.</typeparam>
    /// <param name="map">Mapping to apply</param>
    /// <param name="factory">Factory to create a <see cref="TermNode" /> when adding a mapping</param>
    /// </summary>
    public EnumerableUnaryEngineBuilder<T> MapFrom<TModel>(
        Func<TModel, (bool, string)> map,
        Func<string, string, TermNode> factory
    )
    {
        Action<EnumerableFilterResult<T>, string, TermOption, TModel> mapFrom = (
            EnumerableFilterResult<T> terms,
            string name,
            TermOption termOption,
            TModel model
        ) =>
        {
            (var shouldMap, var value) = map(model);
            if (shouldMap)
            {
                var node = termOption.MapFromFactory(name, value);
                terms.TryAddOrReplace(node);
            }
            else
            {
                terms.TryRemove(name);
            }
        };

        _termOption.MapFrom = mapFrom;
        _termOption.MapFromFactory = factory;

        return this;
    }
}
