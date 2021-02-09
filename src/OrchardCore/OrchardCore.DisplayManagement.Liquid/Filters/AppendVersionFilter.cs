using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public static class AppendVersionFilter
    {
        public static ValueTask<FluidValue> AppendVersion(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var fileVersionProvider = context.Services.GetRequiredService<IFileVersionProvider>();
            var httpContextAccessor = context.Services.GetRequiredService<IHttpContextAccessor>();

            var url = input.ToStringValue();

            var imageUrl = fileVersionProvider.AddFileVersionToPath(httpContextAccessor.HttpContext.Request.PathBase, url);

            return new ValueTask<FluidValue>(new StringValue(imageUrl ?? url));
        }
    }
}
