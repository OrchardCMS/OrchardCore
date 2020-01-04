using System;
using System.Collections.Generic;
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

            var routeData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            routeData.Add("area", "OrchardCore.ContentLocalization");
            routeData.Add("targetCulture", targetCulture);
            routeData.Add("contentItemUrl", request.Path.Value);

            foreach (var kv in request.Query)
            {
                routeData.TryAdd(kv.Key, kv.Value.ToString());
            }

            var url = urlHelper.RouteUrl("RedirectToLocalizedContent", routeData);
            return new ValueTask<FluidValue>(FluidValue.Create(url));
        }
    }
}
