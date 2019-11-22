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
        public override async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ThemeLayout", out dynamic layout))
            {
                throw new ArgumentException("ThemeLayout missing while invoking 'render_body'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out var item) || !(item is IDisplayHelper displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'render_body'");
            }

            IHtmlContent htmlContent = await displayHelper.ShapeExecuteAsync(layout.Content);

            htmlContent.WriteTo(writer, (HtmlEncoder)encoder);
            return Completion.Normal;
        }
    }
}
