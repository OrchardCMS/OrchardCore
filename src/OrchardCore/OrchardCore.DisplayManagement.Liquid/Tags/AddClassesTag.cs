using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class AddClassesTag : ExpressionArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

                var classes = arguments["classes"].Or(arguments.At(0));

                if (classes.Type == FluidValues.String)
                {
                    var values = classes.ToStringValue().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var value in values)
                    {
                        shape.Classes.Add(value);
                    }
                }
                else if (classes.Type == FluidValues.Array)
                {
                    foreach (var value in classes.Enumerate())
                    {
                        shape.Classes.Add(value.ToStringValue());
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
