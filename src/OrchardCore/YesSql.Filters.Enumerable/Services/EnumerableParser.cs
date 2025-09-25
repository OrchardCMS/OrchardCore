using Parlot;
using Parlot.Fluent;
using YesSql.Filters.Abstractions.Nodes;
using static Parlot.Fluent.Parsers;

namespace YesSql.Filters.Enumerable.Services;

public class EnumerableParser<T> : IEnumerableParser<T>
    where T : class
{
    private readonly Dictionary<string, EnumerableTermOption<T>> _termOptions;
    private readonly Parser<EnumerableFilterResult<T>> _parser;

    public EnumerableParser(
        Parser<TermNode>[] termParsers,
        Dictionary<string, EnumerableTermOption<T>> termOptions
    )
    {
        _termOptions = termOptions;

        var terms = OneOf(termParsers);

        _parser = ZeroOrMany(terms)
            .Then(
                static (context, terms) =>
                {
                    var ctx = (EnumerableParseContext<T>)context;

                    return new EnumerableFilterResult<T>((List<TermNode>)terms, ctx.TermOptions);
                }
            )
            .Compile();
    }

    public EnumerableFilterResult<T> Parse(string text)
    {
        if (String.IsNullOrEmpty(text))
        {
            return new EnumerableFilterResult<T>(_termOptions);
        }

        var context = new EnumerableParseContext<T>(_termOptions, new Scanner(text));

        var result = default(ParseResult<EnumerableFilterResult<T>>);
        if (_parser.Parse(context, ref result))
        {
            return result.Value;
        }
        else
        {
            return new EnumerableFilterResult<T>(_termOptions);
        }
    }
}
