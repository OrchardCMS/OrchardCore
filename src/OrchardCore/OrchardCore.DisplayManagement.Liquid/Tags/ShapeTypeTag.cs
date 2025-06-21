using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class ShapeTypeTag
{
    public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await arguments.Item1.EvaluateAsync(context).ConfigureAwait(false)).ToObjectValue();

        if (objectValue is IShape shape)
        {
            var shapeType = (await arguments.Item2.EvaluateAsync(context).ConfigureAwait(false)).ToStringValue();
            shape.Metadata.Type = shapeType;
        }

        return Completion.Normal;
    }
}
