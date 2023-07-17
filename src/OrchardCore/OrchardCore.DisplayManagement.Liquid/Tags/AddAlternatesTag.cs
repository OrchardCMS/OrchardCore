using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class AddAlternatesTag
    {
        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var alternates = await arguments.Item2.EvaluateAsync(context);

                if (alternates.Type == FluidValues.String)
                {
                    var values = alternates.ToStringValue().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var value in values)
                    {
                        shape.Metadata.Alternates.Add(value);
                    }
                }
                else if (alternates.Type == FluidValues.Array)
                {
                    foreach (var value in alternates.Enumerate(context))
                    {
                        shape.Metadata.Alternates.Add(value.ToStringValue());
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
