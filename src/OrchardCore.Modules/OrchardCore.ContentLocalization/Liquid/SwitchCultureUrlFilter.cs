using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public static class SwitchCultureUrlFilter
    {
        public static ValueTask<FluidValue> SwitchCultureUrl(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;
            var urlHelperFactory = context.Services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(context.ViewContext);

            var request = (HttpRequest)ctx.GetValue("Request")?.ToObjectValue();
            if (request == null)
            {
                throw new ArgumentException("HttpRequest missing while invoking 'switch_culture_url'");
            }

            var targetCulture = input.ToStringValue();

            var url = urlHelper.RouteUrl("RedirectToLocalizedContent",
                new
                {
                    area = "OrchardCore.ContentLocalization",
                    targetCulture = targetCulture,
                    contentItemUrl = request.Path.Value,
                    queryStringValue = request.QueryString.Value
                });
            return new ValueTask<FluidValue>(FluidValue.Create(url, ctx.Options));
        }
    }
}
