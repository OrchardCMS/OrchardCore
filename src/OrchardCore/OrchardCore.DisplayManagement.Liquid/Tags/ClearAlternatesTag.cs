using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class ClearAlternatesTag
{
    public static async ValueTask<Completion> WriteToAsync(Expression expression, IFluidOutput _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await expression.EvaluateAsync(context)).ToObjectValue(context);

        if (objectValue is IShape shape && shape.Metadata.Alternates.Count > 0)
        {
            shape.Metadata.Alternates.Clear();
        }

        return Completion.Normal;
    }
}
