using System;
using YesSql;

namespace OrchardCore.Data.QueryParser.Fluent
{
    public static partial class QueryParsers
    {
        public static QueryParser<T> QueryParser<T>(params TermParser<T>[] termParsers) where T : class
            => new QueryParser<T>(termParsers);

        public static NamedTermParser<T> NamedTermParser<T>(string name, OperatorParser<T> operatorParser) where T : class
            => new NamedTermParser<T>(name, operatorParser);

        public static DefaultTermParser<T> DefaultTermParser<T>(string name, OperatorParser<T> operatorParser) where T : class
            => new DefaultTermParser<T>(name, operatorParser);

        public static UnaryParser<T> OneConditionParser<T>(Func<IQuery<T>, string, IQuery<T>> query) where T : class
            => new UnaryParser<T>(query);

        public static BooleanParser<T> ManyConditionParser<T>(Func<IQuery<T>, string, IQuery<T>> matchQuery, Func<IQuery<T>, string, IQuery<T>> notMatchQuery) where T : class
            => new BooleanParser<T>(matchQuery, notMatchQuery);
    }
}