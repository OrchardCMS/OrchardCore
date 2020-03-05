using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace OrchardCore.Liquid.Ast
{
    public class ArgumentsExpression : Expression
    {
        private readonly FilterArgument[] _arguments;

        public ArgumentsExpression(FilterArgument[] arguments)
        {
            _arguments = arguments;
        }

        public override async ValueTask<FluidValue> EvaluateAsync(TemplateContext context)
        {
            var arguments = new FilterArguments();

            foreach (var argument in _arguments)
            {
                arguments.Add(argument.Name, await argument.Expression.EvaluateAsync(context));
            }

            return FluidValue.Create(arguments);
        }
    }
}
