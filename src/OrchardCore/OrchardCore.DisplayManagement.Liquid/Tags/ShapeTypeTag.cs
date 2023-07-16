using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeTypeTag
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter writer, TextEncoder encoder, TemplateContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var shapeType = (await arguments.Item2.EvaluateAsync(context)).ToStringValue();
                shape.Metadata.Type = shapeType;
            }

            return Completion.Normal;
        }
    }
}
