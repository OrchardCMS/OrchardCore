using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class ShapeAddPropertyTag
    {
        public static async ValueTask<Completion> WriteToAsync(ValueTuple<Expression, List<FilterArgument>> arguments, TextWriter _1, TextEncoder _2, TemplateContext context)
        {
            var objectValue = (await arguments.Item1.EvaluateAsync(context)).ToObjectValue();

            if (objectValue is IShape shape)
            {
                var attributes = arguments.Item2;

                foreach (var property in attributes)
                {
                    shape.Properties[property.Name.ToPascalCaseUnderscore()] = (await property.Expression.EvaluateAsync(context)).ToObjectValue();
                }
            }

            return Completion.Normal;
        }
    }
}
