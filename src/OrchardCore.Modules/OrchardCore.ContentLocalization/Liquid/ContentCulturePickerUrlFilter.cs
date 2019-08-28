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
                throw new ArgumentException("UrlHelper missing while invoking 'content_culture_picker_url'");
            }
            var request = (HttpRequest)ctx.GetValue("Request")?.ToObjectValue();
            if (request == null)
            {
                throw new ArgumentException("HttpRequest missing while invoking 'content_culture_picker_url'");
            }

            var targetCulture = input.ToStringValue();
            var urlHelper = (IUrlHelper)urlHelperObj;
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
