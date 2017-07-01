using Fluid;
using Fluid.Values;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddFilter("t", Localize);
            filters.AddFilter("href", UrlContent);

            return filters;
        }

        public static FluidValue Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "t");
            return new StringValue(page.T[input.ToStringValue()].Value);
        }

        public static FluidValue UrlContent(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "href");
            return new StringValue(page.Href(input.ToStringValue()));
        }
    }
}