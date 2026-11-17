using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class ShapePositionTag
{
    public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, IFluidOutput _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue(context);

        if (objectValue is IShape shape)
        {
            var position = (await arguments.Item2.EvaluateAsync(context)).ToStringValue(context);
            shape.Metadata.Position = position;
        }

        return Completion.Normal;
    }

}
