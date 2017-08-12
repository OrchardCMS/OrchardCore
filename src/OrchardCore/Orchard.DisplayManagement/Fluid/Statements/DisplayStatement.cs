using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class DisplayStatement : Statement
    {
        public DisplayStatement(Expression shape)
        {
            Shape = shape;
        }

        public Expression Shape { get; }

        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'display'");
            }

            var shape = (await Shape.EvaluateAsync(context)).ToObjectValue();
            var htmlContent = await (Task<IHtmlContent>)displayHelper(shape);
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}