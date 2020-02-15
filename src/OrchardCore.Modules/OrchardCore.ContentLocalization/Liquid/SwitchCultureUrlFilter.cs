using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public class SwitchCultureUrlFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out var urlHelperObj))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'switch_culture_url'");
            }
            var request = (HttpRequest)ctx.GetValue("Request")?.ToObjectValue();
            if (request == null)
            {
                throw new ArgumentException("HttpRequest missing while invoking 'switch_culture_url'");
            }

            var targetCulture = input.ToStringValue();
            var urlHelper = (IUrlHelper)urlHelperObj;

            var url = urlHelper.RouteUrl("RedirectToLocalizedContent",
                new
                {
                    area = "OrchardCore.ContentLocalization",
                    targetCulture = targetCulture,
                    contentItemUrl = request.Path.Value,
                    queryStringValue = request.QueryString.Value
                });
            return new ValueTask<FluidValue>(FluidValue.Create(url));
        }
    }
}
