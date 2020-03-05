using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;

namespace OrchardCore.Liquid.Ast
{
    public abstract class ExpressionArgumentsTag : ITag
    {
        public BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.Expression + grammar.FilterArguments;
        }

        public Statement Parse(ParseTreeNode node, ParserContext context)
        {
            var expression = DefaultFluidParser.BuildExpression(node.ChildNodes[0].ChildNodes[0]);
            var arguments = node.ChildNodes[0].ChildNodes[1].ChildNodes.Select(DefaultFluidParser.BuildFilterArgument).ToArray();
            return new DelegateStatement((writer, encoder, ctx) => WriteToAsync(writer, encoder, ctx, expression, arguments));
        }

        public abstract ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression term, FilterArgument[] arguments);
    }
}
