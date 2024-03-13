using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public class SwitchCultureUrlFilter : ILiquidFilter
    {
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SwitchCultureUrlFilter(IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor)
        {
            _urlHelperFactory = urlHelperFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(context.ViewContext);

            var request = _httpContextAccessor.HttpContext?.Request
                ?? throw new ArgumentException("HttpRequest missing while invoking 'switch_culture_url'");

            var targetCulture = input.ToStringValue();

            var url = urlHelper.RouteUrl("RedirectToLocalizedContent",
                new
                {
                    area = "OrchardCore.ContentLocalization",
                    targetCulture,
                    contentItemUrl = request.Path.Value,
                    queryStringValue = request.QueryString.Value,
                });
            return new ValueTask<FluidValue>(FluidValue.Create(url, context.Options));
        }
    }
}
