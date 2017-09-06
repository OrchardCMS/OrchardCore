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
    public class RenderBodyTag : SimpleTag
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ThemeLayout", out dynamic layout))
            {
                throw new ArgumentException("ThemeLayout missing while invoking 'render_body'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'render_body'");
            }

            var htmlContent = await (Task<IHtmlContent>)displayHelper(layout.Content);
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}