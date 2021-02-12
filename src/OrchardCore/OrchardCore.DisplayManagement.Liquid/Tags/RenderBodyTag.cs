using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class RenderBodyTag
    {
        public static async ValueTask<Completion> WriteToAsync(TextWriter writer, TextEncoder encoder, TemplateContext context)
        {
            var services = ((LiquidTemplateContext)context).Services;

            dynamic layout = await services.GetRequiredService<ILayoutAccessor>().GetLayoutAsync();
            var displayHelper = services.GetRequiredService<IDisplayHelper>();

            IHtmlContent htmlContent = await displayHelper.ShapeExecuteAsync(layout.Content);

            htmlContent.WriteTo(writer, (HtmlEncoder)encoder);
            return Completion.Normal;
        }
    }
}
