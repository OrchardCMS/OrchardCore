using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;

namespace Orchard.Liquid.Filters
{
    public class ContentUrlFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            object urlHelper;
            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out urlHelper))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'displayUrl'");
            }

            return Task.FromResult<FluidValue>(new StringValue(((IUrlHelper)urlHelper).Content(input.ToStringValue())));
        }
    }
}
