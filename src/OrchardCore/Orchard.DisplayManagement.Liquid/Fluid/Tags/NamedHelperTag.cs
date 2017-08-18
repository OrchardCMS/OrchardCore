using System.Linq;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;
using Orchard.DisplayManagement.Fluid.Ast;

namespace Orchard.DisplayManagement.Fluid.Tags
{
    public class NamedHelperTag : ITag
    {
        public BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.FilterArguments;
        }

        public Statement Parse(ParseTreeNode node, ParserContext context)
        {
            var arguments = node.ChildNodes[0].ChildNodes.Select(DefaultFluidParser.BuildFilterArgument).ToArray();
            return new HelperStatement(new ArgumentsExpression(arguments), node.Term.Name);
        }
    }

    public class NamedHelperBlock : CustomBlock
    {
        public override BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.FilterArguments;
        }

        public override Statement Parse(ParseTreeNode node, ParserContext context)
        {
            var tag = context.CurrentBlock.Tag;
            var e = context.CurrentBlock.Tag.ChildNodes[0];
            var arguments = e.ChildNodes.Select(DefaultFluidParser.BuildFilterArgument).ToArray();
            var statements = context.CurrentBlock.Statements;
            return new HelperStatement(new ArgumentsExpression(arguments), tag.Term.Name, statements);
        }
    }
}