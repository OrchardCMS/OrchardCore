using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using OrchardCore.DisplayManagement.Liquid.Ast;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapePagerTag : ExpressionArgumentsTag
    {
        private static readonly string[] _properties = { "PreviousText", "NextText", "PreviousClass", "NextClass" };

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue() as dynamic;

            if (objectValue is Shape shape && shape.Metadata.Type == "PagerSlim")
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();


                foreach (var name in arguments.Names)
                {
                    var argument = arguments[name];
                    var propertyName = LiquidViewFilters.LowerKebabToPascalCase(name);

                    if (_properties.Contains(propertyName))
                    {
                        objectValue[propertyName] = argument.ToObjectValue();
                    }
                    objectValue[propertyName] = argument.ToObjectValue();
                }
            }

            return Completion.Normal;
        }
    }
}