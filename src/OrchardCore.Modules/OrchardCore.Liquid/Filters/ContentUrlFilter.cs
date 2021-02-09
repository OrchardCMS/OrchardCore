using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Liquid.Filters
{
    public static class ContentUrlFilter
    {
        public static ValueTask<FluidValue> Href(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;
            var urlHelperFactory = context.Services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(context.ViewContext);

            return new ValueTask<FluidValue>(new StringValue(((IUrlHelper)urlHelper).Content(input.ToStringValue())));
        }
    }
}
