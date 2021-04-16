
using System;
using System.Collections.Generic;
using OrchardCore.Filters.Abstractions.Nodes;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;


namespace OrchardCore.Filters.Query.Services
{
    public class QueryParser<T> : IQueryParser<T> where T : class
    {
        private Dictionary<string, QueryTermOption<T>> _termOptions;
        private Parser<QueryFilterResult<T>> _parser;

        public QueryParser(Parser<TermNode>[] termParsers, Dictionary<string, QueryTermOption<T>> termOptions)
        {
            _termOptions = termOptions;

            var terms = OneOf(termParsers);

            _parser = ZeroOrMany(terms)
                    .Then((context, terms) => 
                    {
                        var ctx = (QueryParseContext<T>)context;

                        return new QueryFilterResult<T>(terms, ctx.TermOptions);
                    });                    
        }

        public QueryFilterResult<T> Parse(string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return new QueryFilterResult<T>(_termOptions);
            }

            var context = new QueryParseContext<T>(_termOptions, new Scanner(text));

            ParseResult<QueryFilterResult<T>> result = default(ParseResult<QueryFilterResult<T>>);
            if (_parser.Parse(context, ref result))
            {
                return result.Value;
            }
            else
            {
                return new QueryFilterResult<T>(_termOptions);
            }
        }
    }
}