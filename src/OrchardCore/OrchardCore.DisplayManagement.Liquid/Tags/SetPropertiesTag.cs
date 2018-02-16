using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using OrchardCore.DisplayManagement.Liquid.Ast;
using OrchardCore.DisplayManagement.Liquid.Filters;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class SetPropertiesTag : ExpressionArgumentsTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var obj = (await expression.EvaluateAsync(context)).ToObjectValue() as dynamic;

            if (obj != null)
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

                foreach (var name in arguments.Names)
                {
                    var argument = arguments[name];
                    var propertyName = LiquidViewFilters.LowerKebabToPascalCase(name);

                    if (argument.Type == FluidValues.Array)
                    {
                        var values = argument.Enumerate();

                        if (values.Count() > 0)
                        {
                            var type = values.First().Type;

                            if (type == FluidValues.String)
                            {
                                obj[propertyName] = values.Select(v => v.ToStringValue());
                            }
                            else if (type == FluidValues.Number)
                            {
                                obj[propertyName] = values.Select(v => v.ToNumberValue());
                            }
                            else if (type == FluidValues.Boolean)
                            {
                                obj[propertyName] = values.Select(v => v.ToBooleanValue());
                            }
                            else
                            {
                                obj[propertyName] = values.Select(v => v.ToObjectValue());
                            }
                        }
                    }
                    else
                    {
                        obj[propertyName] = argument.ToObjectValue();
                    }
                }
            }

            return Completion.Normal;
        }
    }
}