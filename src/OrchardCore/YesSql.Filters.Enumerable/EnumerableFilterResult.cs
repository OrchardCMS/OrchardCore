using YesSql.Filters.Abstractions.Nodes;
using YesSql.Filters.Abstractions.Services;
using YesSql.Filters.Enumerable.Services;

namespace YesSql.Filters.Enumerable;

public class EnumerableFilterResult<T> : FilterResult<T, EnumerableTermOption<T>>
    where T : class
{
    public EnumerableFilterResult(IReadOnlyDictionary<string, EnumerableTermOption<T>> termOptions)
        : base(termOptions) { }

    public EnumerableFilterResult(
        List<TermNode> terms,
        IReadOnlyDictionary<string, EnumerableTermOption<T>> termOptions
    )
        : base(terms, termOptions) { }

    public void MapFrom<TModel>(TModel model)
    {
        foreach (var option in TermOptions)
        {
            if (
                option.Value.MapFrom
                is Action<EnumerableFilterResult<T>, string, TermOption, TModel> mappingMethod
            )
            {
                mappingMethod(this, option.Key, option.Value, model);
            }
        }
    }

    /// <summary>
    /// Applies term filters to an <see cref="IEnumerable{T}"/>
    /// </summary>
    public async ValueTask<IEnumerable<T>> ExecuteAsync(EnumerableExecutionContext<T> context)
    {
        var visitor = new EnumerableFilterVisitor<T>();

        foreach (var term in _terms.Values)
        {
            // TODO optimize value task.
            await VisitTerm(TermOptions, context, visitor, term);
        }

        // Execute always run terms. These are not added to the terms list.
        foreach (var termOption in TermOptions)
        {
            if (!termOption.Value.AlwaysRun)
            {
                continue;
            }

            if (!_terms.ContainsKey(termOption.Key))
            {
                var alwaysRunNode = new NamedTermNode(
                    termOption.Key,
                    new UnaryNode(String.Empty, OperateNodeQuotes.None)
                );
                await VisitTerm(TermOptions, context, visitor, alwaysRunNode);
            }
        }

        return context.Item;
    }

    private static async Task VisitTerm(
        IReadOnlyDictionary<string, EnumerableTermOption<T>> termOptions,
        EnumerableExecutionContext<T> context,
        EnumerableFilterVisitor<T> visitor,
        TermNode term
    )
    {
        context.CurrentTermOption = termOptions[term.TermName];

        var termQuery = visitor.Visit(term, context);
        context.Item = await termQuery.Invoke(context.Item);
        context.CurrentTermOption = null;
    }
}
