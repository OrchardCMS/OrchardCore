using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Irony.Parsing;

namespace Orchard.DisplayManagement.Fluid.Ast
{
    public class ArgumentsExpression : Expression
    {
        private readonly FilterArgument[] _arguments;

        public ArgumentsExpression(FilterArgument[] arguments)
        {
            _arguments = arguments;
        }

        public override async Task<FluidValue> EvaluateAsync(TemplateContext context)
        {
            var arguments = new FilterArguments();

            foreach(var argument in _arguments)
            {
                arguments.Add(argument.Name, await argument.Expression.EvaluateAsync(context));
            }

            return FluidValue.Create(arguments);
        }

        public static ArgumentsExpression Build(ParseTreeNode node)
        {
            return new ArgumentsExpression(node.ChildNodes.Select(DefaultFluidParser.BuildFilterArgument).ToArray());
        }
    }
}