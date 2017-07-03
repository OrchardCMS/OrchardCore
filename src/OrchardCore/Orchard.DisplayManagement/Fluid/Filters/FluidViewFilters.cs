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
            filters.AddFilter("ordefault", OrDefault);
            filters.AddFilter("bodyaspect", BodyAspect);

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

        public static FluidValue OrDefault(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "ordefault");
            var output = page.OrDefault(input.ToObjectValue(), arguments.At(0).ToObjectValue()).ToString();
            return new StringValue(output);
        }

        public static FluidValue BodyAspect(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "bodyaspect");
            return new ObjectValue(page.New.BodyAspect(input.ToObjectValue()));
        }
    }
}