using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;

namespace OrchardCore.Liquid.Filters
{
    public class ContentUrlFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out var urlHelper))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'href'");
            }

            return Task.FromResult<FluidValue>(new StringValue(((IUrlHelper)urlHelper).Content(input.ToStringValue())));
        }
    }
}
