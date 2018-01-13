using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Liquid.Filters
{
    public class AbsoluteUrlFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var relativePath = input.ToStringValue();

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.FromResult(input);
            }

            if (!context.AmbientValues.TryGetValue("UrlHelper", out var urlHelper))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'display_url'");
            }

            var result = new StringValue(((IUrlHelper)urlHelper).ToAbsoluteUrl(relativePath));
            return Task.FromResult<FluidValue>(result);
        }
    }
}
