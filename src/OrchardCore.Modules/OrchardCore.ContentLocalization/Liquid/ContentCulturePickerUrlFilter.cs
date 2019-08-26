using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public class ContentCulturePickerUrlFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out var urlHelperObj))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'signal_url'");
            }

            var targetCulture = input.ToStringValue();
            var urlHelper = (IUrlHelper)urlHelperObj;
            var request = (HttpRequest)ctx.GetValue("Request").ToObjectValue();
            var url = urlHelper.Action("RedirectToLocalizedContent",
                "ContentculturePicker",
                new
                {
                    area = "OrchardCore.ContentLocalization",
                    targetculture = targetCulture,
                    contentItemUrl = request.Path,
                });
            return new ValueTask<FluidValue>(FluidValue.Create(url));
         }
    }
}
