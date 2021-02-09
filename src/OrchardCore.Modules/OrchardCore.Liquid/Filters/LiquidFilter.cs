using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Liquid.Filters
{
    public static class LiquidFilter
    {
        public static async ValueTask<FluidValue> Liquid(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;
            var liquidTemplateManager = context.Services.GetRequiredService<ILiquidTemplateManager>();
            var htmlEncoder = context.Services.GetRequiredService<HtmlEncoder>();

            var content = await liquidTemplateManager.RenderAsync(input.ToStringValue(), htmlEncoder, arguments.At(0));
            return new StringValue(content, false);
        }
    }
}
