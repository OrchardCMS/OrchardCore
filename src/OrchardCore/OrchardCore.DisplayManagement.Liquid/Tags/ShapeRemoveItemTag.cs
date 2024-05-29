using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeRemoveItemTag
    {
        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is Shape shape && shape.Items != null)
            {
                var shapeName = (await arguments.Item2.EvaluateAsync(context)).ToStringValue();
                shape.Remove(shapeName);
            }

            return Completion.Normal;
        }
    }
}
