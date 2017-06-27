using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;

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
            if (!context.AmbientValues.TryGetValue("FluidPage", out var view))
            {
                throw new ParseException("FluidPage missing while invoking 't'.");
            }

            return new StringValue(((FluidPage)view).T[input.ToStringValue()].Value);
        }

        public static FluidValue UrlContent(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("UrlHelper", out var urlHelper))
            {
                throw new ParseException("UrlHelper missing while invoking 'url_content'.");
            }

            return new StringValue(((IUrlHelper)urlHelper).Content(input.ToStringValue()));
        }
    }
}