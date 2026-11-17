using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class ShapeRemoveItemTag
{
    public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, IFluidOutput _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue(context);

        if (objectValue is Shape shape && shape.Items != null)
        {
            var shapeName = (await arguments.Item2.EvaluateAsync(context)).ToStringValue(context);
            shape.Remove(shapeName);
        }

        return Completion.Normal;
    }
}
