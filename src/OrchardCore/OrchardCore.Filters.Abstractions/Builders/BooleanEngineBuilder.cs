
using OrchardCore.Filters.Abstractions.Nodes;
using OrchardCore.Filters.Abstractions.Services;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.Filters.Abstractions.Builders
{
    public abstract class BooleanEngineBuilder<T, TTermOption> : OperatorEngineBuilder<T, TTermOption> where TTermOption : TermOption
    {
        private static Parser<OperatorNode> _parser;
        protected TTermOption _termOption;

        static BooleanEngineBuilder()
        {
            var OperatorNode = Deferred<OperatorNode>();

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

            var GroupNode = Between(Terms.Char('('), OperatorNode, Terms.Char(')'))
                .Then<OperatorNode>(x => new GroupNode(x));

            var SingleNode = Terms.String() // A term name is never enclosed in strings.
                .Or(
                    // This must be aborted when it is consuming the next term.
                    Terms.Identifier().AndSkip(Not(Literals.Char(':'))) // TODO when this is NonWhiteSpace it sucks up paranthese. Will Identifier catch accents, i.e. multilingual.
                )
                    .Then<OperatorNode>((node) => new UnaryNode(node.ToString()));

            var Primary = SingleNode.Or(GroupNode);

            var UnaryNode = NotOperator.And(Primary)
                .Then<OperatorNode>((node) =>
                {
                    // mutate with the neg query.
                    var unaryNode = node.Item2 as UnaryNode;

                    // TODO test what actually happens when just using NOT foo
                    return new NotUnaryNode(node.Item1, new UnaryNode(unaryNode.Value, false));
                })
                .Or(Primary);

            var AndNode = UnaryNode.And(ZeroOrMany(AndOperator.And(UnaryNode)))
                .Then<OperatorNode>(node =>
                {
                    // unary
                    var result = node.Item1;

                    foreach (var op in node.Item2)
                    {
                        result = new AndNode(result, op.Item2, op.Item1);
                    }

                    return result;
                });

            OperatorNode.Parser = AndNode.And(ZeroOrMany(NotOperator.Or(OrOperator).And(AndNode)))
               .Then<OperatorNode>((node) =>
               {
                   static NotNode CreateNotNode(OperatorNode result, (string, OperatorNode) op)
                       => new NotNode(result, new UnaryNode(((UnaryNode)op.Item2).Value, false), op.Item1);

                   static OrNode CreateOrNode(OperatorNode result, (string, OperatorNode) op)
                       => new OrNode(result, op.Item2, op.Item1);

                   // unary
                   var result = node.Item1;

                   foreach (var op in node.Item2)
                   {
                       result = op.Item1 switch
                       {
                           "NOT" => CreateNotNode(result, op),
                           "!" => CreateNotNode(result, op),
                           "OR" => CreateOrNode(result, op),
                           "||" => CreateOrNode(result, op),
                           " " => CreateOrNode(result, op),
                           _ => null
                       };
                   }

                   return result;
               });

            _parser = OperatorNode;

        }

        public override (Parser<OperatorNode> Parser, TTermOption TermOption) Build()
            => (_parser, _termOption);
    }
}
