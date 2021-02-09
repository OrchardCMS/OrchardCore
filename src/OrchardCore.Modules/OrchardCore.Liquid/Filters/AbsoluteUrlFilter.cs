using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Liquid.Filters
{
    public static class AbsoluteUrlFilter
    {
        public static ValueTask<FluidValue> AbsoluteUrl(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var relativePath = input.ToStringValue();

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return new ValueTask<FluidValue>(input);
            }

            var context = (LiquidTemplateContext)ctx;
            var urlHelperFactory = context.Services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(context.ViewContext);

            var result = new StringValue(urlHelper.ToAbsoluteUrl(relativePath));
            return new ValueTask<FluidValue>(result);
        }
    }
}
