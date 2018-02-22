using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class DisplayTag : ExpressionTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context, Expression expression)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_display'");
            }

            var shape = (await expression.EvaluateAsync(context)).ToObjectValue();
            var htmlContent = await (Task<IHtmlContent>)displayHelper(shape);
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}