using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class ShapeTabTag
{
    public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

        if (objectValue is IShape shape)
        {
            var shapeTab = (await arguments.Item2.EvaluateAsync(context)).ToStringValue();
            shape.Metadata.Tab = shapeTab;
        }

        return Completion.Normal;
    }
}
