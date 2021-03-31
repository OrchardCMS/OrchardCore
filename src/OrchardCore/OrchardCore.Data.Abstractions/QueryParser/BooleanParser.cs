using Parlot;
using Parlot.Fluent;
using System;
using YesSql;
using YesSql.Indexes;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Data.QueryParser
{

    public class BooleanParser<T> : OperatorParser<T> where T : class
    {
        public BooleanParser(Func<IQuery<T>, string, IQuery<T>> matchQuery, Func<IQuery<T>, string, IQuery<T>> notMatchQuery)
        {
            var OperatorNode = Deferred<OperatorNode<T>>();

            var AndOperator = Terms.Text("AND")
                .Or(
                    Literals.Text("&&")
                );

            var NotOperator = Terms.Text("NOT")
                .Or(
                    Literals.Text("!")
                );

            var OrTextOperators = Terms.Text("OR")
                .Or(
                    Terms.Text("||")
                );

            // Operators that need to be NOT next when the default OR ' ' operator is found.
            var NotOrOperators = OneOf(AndOperator, NotOperator, OrTextOperators);

            // Default operator.
            var OrOperator = Literals.Text(" ").AndSkip(Not(NotOrOperators))// With this is is now catching everything.
                .Or(
                    OrTextOperators
                );

            var GroupNode = Between(Terms.Char('('), OperatorNode, Terms.Char(')'));

            var SingleNode = Terms.String() // A term name is never enclosed in strings.
                .Or(
                    // This must be aborted when it is consuming the next term.
                    Terms.Identifier().AndSkip(Not(Literals.Char(':'))) // TODO when this is NonWhiteSpace it sucks up paranthese. Will Identifier catch accents, i.e. multilingual.
                )
                    .Then<OperatorNode<T>>(x => new UnaryNode<T>(x.ToString(), matchQuery));

            var Primary = SingleNode.Or(GroupNode);

            var UnaryNode = NotOperator.And(Primary)
                .Then<OperatorNode<T>>(x =>
                {
                    // mutate with the neg query.
                    var unaryNode = x.Item2 as UnaryNode<T>;

                    // TODO test what actually happens when just using NOT foo
                    return new NotUnaryNode<T>(x.Item1, new UnaryNode<T>(unaryNode.Value, notMatchQuery));
                })
                .Or(Primary);

            var AndNode = UnaryNode.And(ZeroOrMany(AndOperator.And(UnaryNode)))
                .Then<OperatorNode<T>>(x =>
                {
                    // unary
                    var result = x.Item1;

                    foreach (var op in x.Item2)
                    {
                        result = new AndNode<T>(result, op.Item2, op.Item1);
                    }

                    return result;
                });

            OperatorNode.Parser = AndNode.And(ZeroOrMany(NotOperator.Or(OrOperator).And(AndNode)))
               .Then<OperatorNode<T>>(x =>
               {
                    // unary
                    var result = x.Item1;

                   foreach (var op in x.Item2)
                   {
                       result = op.Item1 switch
                       {
                           "NOT" => new NotNode<T>(result, new UnaryNode<T>(((UnaryNode<T>)op.Item2).Value, notMatchQuery), op.Item1),
                           "!" => new NotNode<T>(result, new UnaryNode<T>(((UnaryNode<T>)op.Item2).Value, notMatchQuery), op.Item1),
                           "OR" => new OrNode<T>(result, op.Item2, op.Item1),
                           "||" => new OrNode<T>(result, op.Item2, op.Item1),
                           " " => new OrNode<T>(result, op.Item2, op.Item1),
                           _ => null
                       };
                   }

                   return result;
               });

            Parser = OperatorNode;

        }

        protected Parser<OperatorNode<T>> Parser { get; private set; }

        public override bool Parse(ParseContext context, ref ParseResult<OperatorNode<T>> result)
        {
            context.EnterParser(this);

            return Parser.Parse(context, ref result);
        }
    }
}
