using System;
using System.Collections.Generic;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc.Utilities;
using Newtonsoft.Json.Linq;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddFilter("t", Localize);
            filters.AddFilter("href", Href);
            filters.AddFilter("is_nil", IsNil);
            filters.AddFilter("or_default", OrDefault);
            filters.AddFilter("body_aspect", BodyAspect);
            filters.AddFilter("item_remove", ItemRemove);
            filters.AddFilter("item_named", ItemNamed);
            filters.AddFilter("item_prefixed", ItemPrefixed);
            filters.AddFilter("shape_string", ShapeString);
            filters.AddFilter("remove_tags", RemoveTags);
            filters.AddFilter("prop_named", PropNamed);
            filters.AddFilter("date_time", DateTime);

            return filters;
        }

        public static FluidValue Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "t");
            var localizer = page.GetService<IStringLocalizer<FluidPage>>();

            var parameters = new List<object>();
            for (var i = 0; i < arguments.Count; i++)
            {
                parameters.Add(arguments.At(i).ToStringValue());
            }

            return new StringValue(localizer[input.ToStringValue(), parameters.ToArray()].Value);
        }

        public static FluidValue Href(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "href");
            return new StringValue(page.Href(input.ToStringValue()));
        }

        public static FluidValue IsNil(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return new BooleanValue(input is NilValue);
        }

        public static FluidValue OrDefault(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "or_default");
            var output = page.OrDefault(input.ToObjectValue(), arguments.At(0).ToObjectValue()).ToString();
            return new StringValue(output);
        }

        public static FluidValue BodyAspect(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "body_aspect");
            return new ObjectValue(page.New.BodyAspect(input.ToObjectValue()));
        }

        public static FluidValue ItemRemove(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;
            shape?.Remove(arguments.At(0).ToStringValue());
            return input;
        }

        public static FluidValue ItemNamed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;
            return new ObjectValue(shape?.Named(arguments.At(0).ToStringValue()));
        }

        public static FluidValue ItemPrefixed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;

            if (shape != null)
            {
                foreach (var item in shape.Items)
                {
                    var prefixed = item as IShape;

                    if (prefixed != null && prefixed.Metadata.Prefix == arguments.At(0).ToStringValue())
                    {
                        return new ObjectValue(prefixed);
                    }
                }
            }

            return NilValue.Instance;
        }

        public static FluidValue ShapeString(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "shape_string");
            return new StringValue(page.DisplayAsync(input.ToObjectValue()).GetAwaiter().GetResult().ToString());
        }

        public static FluidValue RemoveTags(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var htmlDecode = arguments.HasNamed("html_decode") ? arguments["html_decode"].ToBooleanValue() : false;
            return new StringValue(input.ToStringValue().RemoveTags(htmlDecode));
        }

        public static FluidValue PropNamed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return new ObjectValue(((dynamic)input.ToObjectValue())[arguments.At(0).ToStringValue()]);
        }

        public static FluidValue DateTime(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var dateTime = input.ToObjectValue() as DateTime?;

            if (dateTime.HasValue)
            {
                var page = FluidViewTemplate.EnsureFluidPage(context, "date_time");

                Shape shape = arguments.HasNamed("format")
                    ? page.New.DateTime(Utc: dateTime, Format: arguments["format"].ToStringValue())
                    : page.New.DateTime(Utc: dateTime);

                return new StringValue(page.DisplayAsync(shape).GetAwaiter().GetResult().ToString());
            }

            return input;
        }
    }
}