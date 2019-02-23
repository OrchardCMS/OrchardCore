using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;
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
            context.AmbientValues.TryGetValue("TemplateType", out var templateType);

            using (var sw = new StringWriter())
            {
                await manager.RenderAsync(template, sw, templateType != null && templateType.ToString() == "Query" ? NullEncoder.Default : HtmlEncoder.Default, context);
                return sw.ToString();
            }
        }
    }
}
