using System.Text.Encodings.Web;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags;

public class AddAttributesTag
{
    public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, IReadOnlyList<FilterArgument>> arguments, IFluidOutput _1, TextEncoder _2, TemplateContext context)
    {
        var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue(context);

        if (objectValue is IShape shape)
        {
            var attributes = arguments.Item2;

            foreach (var attribute in attributes)
            {
                shape.Attributes[attribute.Name.Replace('_', '-')] = (await attribute.Expression.EvaluateAsync(context)).ToStringValue(context);
            }
        }

        return Completion.Normal;
    }
}
