using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddFilter("t", Localize);
            filters.AddFilter("url_content", UrlContent);

            return filters;
        }

        public static FluidValue Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "t");
            return new StringValue(page.T[input.ToStringValue()].Value);
        }

        public static FluidValue UrlContent(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "url_content");
            return new StringValue(page.Url.Content(input.ToStringValue()));
        }
    }
}