using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;

namespace OrchardCore.Liquid
{
    public interface ILiquidTemplateManager
    {
        Task RenderAsync(string template, TextWriter textWriter, TextEncoder encoder, TemplateContext context);
    }

    public static class LiquidTemplateManagerExtensions
    {
        public static async Task<string> RenderAsync(this ILiquidTemplateManager manager, string template, TemplateContext context)
        {
            using (var sw = new StringWriter())
            {
                await manager.RenderAsync(template, sw, HtmlEncoder.Default, context);
                return sw.ToString();
            }
        }
    }
}
