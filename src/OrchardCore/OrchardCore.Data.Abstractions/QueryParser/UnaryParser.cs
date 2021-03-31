using Parlot;
using Parlot.Fluent;
using System;
using System.Threading.Tasks;
using YesSql.Indexes;
using YesSql;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Data.QueryParser
{
    public abstract class OperatorParser<T> : Parser<OperatorNode<T>> where T : class
    { }

    public class UnaryParser<T> : OperatorParser<T> where T : class
    {
        public UnaryParser(Func<IQuery<T>, string, IQuery<T>> query)
        {
            Parser = Terms.String()
                .Or(
                    Terms.NonWhiteSpace()
                )
                    .Then<OperatorNode<T>>(x => new UnaryNode<T>(x.ToString(), query)); // instance based. instance constructed only once per parser set.
        }

        protected Parser<OperatorNode<T>> Parser { get; private set; }

        public override bool Parse(ParseContext context, ref ParseResult<OperatorNode<T>> result)
        {
            context.EnterParser(this);

            return Parser.Parse(context, ref result);
        }
    }
}
