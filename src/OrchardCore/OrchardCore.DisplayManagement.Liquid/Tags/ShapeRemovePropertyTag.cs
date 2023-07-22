using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeRemovePropertyTag
    {
        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, Expression> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var propName = (await arguments.Item2.EvaluateAsync(context)).ToStringValue();

                if (!String.IsNullOrEmpty(propName))
                {
                    shape.Properties.Remove(propName.ToPascalCaseUnderscore());
                }
            }

            return Completion.Normal;
        }
    }
}
