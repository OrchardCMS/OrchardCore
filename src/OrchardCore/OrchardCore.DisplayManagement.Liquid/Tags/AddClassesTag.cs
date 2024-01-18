using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class AddClassesTag
    {
        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var classes = await arguments.Item2.EvaluateAsync(context);

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
                    foreach (var value in classes.Enumerate(context))
                    {
                        shape.Classes.Add(value.ToStringValue());
                    }
                }
            }

            return Completion.Normal;
        }
    }
}
