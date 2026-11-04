using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using Fluid.Values;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class AddWrappersTag
{
    public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, IFluidOutput _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue(context);

        if (objectValue is IShape shape)
        {
            var wrappers = (await arguments.Item2.EvaluateAsync(context));

            if (wrappers.Type == FluidValues.String)
            {
                var values = wrappers.ToStringValue(context).Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var value in values)
                {
                    shape.Metadata.Wrappers.Add(value);
                }
            }
            else if (wrappers.Type == FluidValues.Array)
            {
                await foreach (var value in wrappers.EnumerateAsync(context))
                {
                    shape.Metadata.Wrappers.Add(value.ToStringValue(context));
                }
            }
        }

        return Completion.Normal;
    }
}
