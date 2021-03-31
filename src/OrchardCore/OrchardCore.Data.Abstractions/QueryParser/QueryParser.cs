using Parlot;
using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Data.QueryParser
{
    public interface ITermParserProvider<T> where T : class
    {
        IEnumerable<TermParser<T>> GetTermParsers();
    }


    public interface IQueryParser<T> where T : class
    {
        TermList<T> Parse(string text);
    }

    public class QueryParser<T> : IQueryParser<T> where T : class
    {
        private static Parser<List<V>> _customSeparated<U, V>(Parser<U> separator, Parser<V> parser) => new CustomSeparated<U, V>(separator, parser);

        public QueryParser(params TermParser<T>[] parsers)
        {
            var Terms = OneOf(parsers);

            var Seperator = OneOf(parsers.Select(x => x.SeperatorParser).ToArray());

            Parser = _customSeparated(
                Seperator,
                Terms)
                    .Then(static x => new TermList<T>(x));
        }

        protected Parser<TermList<T>> Parser { get; }

        public TermList<T> Parse(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new TermList<T>();
            }

            var context = new ParseContext(new Scanner(text));

            ParseResult<TermList<T>> result = default(ParseResult<TermList<T>>);
            if (Parser.Parse(context, ref result))
            {
                return result.Value;
            }
            else
            {
                return new TermList<T>();
            }
        }
    }
}
