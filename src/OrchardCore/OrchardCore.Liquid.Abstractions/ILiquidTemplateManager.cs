using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;

namespace OrchardCore.Liquid
{
    public interface ILiquidTemplateManager
    {
        Task RenderAsync(string template, TextWriter textWriter, TextEncoder encoder, TemplateContext context);
        bool Validate(string template, out IEnumerable<string> errors);
    }

    public static class LiquidTemplateManagerExtensions
    {
        public static Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TemplateContext context)
        {
            return manager.RenderAsync(template, HtmlEncoder.Default, context);
        }

        public static async Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TextEncoder encoder, TemplateContext context)
        {
            using (var sw = new StringWriter())
            {
                await manager.RenderAsync(template, sw, encoder, context);
                return sw.ToString();
            }
        }
    }
}
