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
    public class AddWrappersTag : ExpressionArgumentsTag
    {
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression, FilterArgument[] args)
        {
            var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var arguments = (FilterArguments)(await new ArgumentsExpression(args).EvaluateAsync(context)).ToObjectValue();

                var alternates = arguments["wrappers"].Or(arguments.At(0));

                if (alternates.Type == FluidValues.String)
                {
                    var values = alternates.ToStringValue().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var value in values)
                    {
                        shape.Metadata.Wrappers.Add(value);
                    }
                }
                else if (alternates.Type == FluidValues.Array)
                {
                    foreach (var value in alternates.Enumerate())
                    {
                        shape.Metadata.Wrappers.Add(value.ToStringValue());
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
