using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace Orchard.DisplayManagement.Fluid.Ast
{
    public class FilterArgumentsExpression : Expression
    {
        private readonly FilterArgument[] _arguments;

        public FilterArgumentsExpression(FilterArgument[] arguments)
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
    }
}