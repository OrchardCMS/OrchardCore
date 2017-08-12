using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Layout;

namespace Orchard.DisplayManagement.Fluid.Statements
{
    public class RenderBodyStatement : Statement
    {
        public override async Task<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'render_body'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'render_body'");
            }

            var layout = ((IServiceProvider)services).GetRequiredService<ILayoutAccessor>().GetLayout();
            var htmlContent = await (Task<IHtmlContent>)displayHelper(layout.Content);
            htmlContent.WriteTo(writer, HtmlEncoder.Default);
            return Completion.Normal;
        }
    }
}