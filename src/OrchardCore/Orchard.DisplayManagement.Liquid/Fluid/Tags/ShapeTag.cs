using System.Linq;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Tags
{
    public class ShapeTag : ITag
    {
        public BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.FilterArguments;
        }

        public Statement Parse(ParseTreeNode node, ParserContext context)
        {
            var arguments = node.ChildNodes[0].ChildNodes.Select(DefaultFluidParser.BuildFilterArgument).ToArray();

            var typeNode = node.ChildNodes[0].ChildNodes[0].ChildNodes[0];

            if (typeNode.Term.Name != "identifier")
            {
                arguments[0] = new FilterArgument("type", DefaultFluidParser.BuildTermExpression(typeNode));
            }

            return new HelperStatement(new ArgumentsExpression(arguments), node.Term.Name);
        }
    }
}