using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using Orchard.DisplayManagement.Shapes;
using Orchard.Mvc.Utilities;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("t", Localize);
            filters.AddAsyncFilter("href", Href);
            filters.AddAsyncFilter("is_nil", IsNil);
            filters.AddAsyncFilter("or_default", OrDefault);
            filters.AddAsyncFilter("i", ItemNamed);
            filters.AddAsyncFilter("item_prefixed", ItemPrefixed);
            filters.AddAsyncFilter("shape_string", ShapeString);
            filters.AddAsyncFilter("remove_tags", RemoveTags);
            filters.AddAsyncFilter("date_time", DateTime);
            filters.AddAsyncFilter("p", PropNamed);

            return filters;
        }

        public static Task<FluidValue> Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "t");
            var localizer = page.GetService<IStringLocalizer<FluidPage>>();

            var parameters = new List<object>();
            for (var i = 0; i < arguments.Count; i++)
            {
                parameters.Add(arguments.At(i).ToStringValue());
            }

            return Task.FromResult<FluidValue>(new StringValue(localizer[input.ToStringValue(), parameters.ToArray()].Value));
        }

        public static Task<FluidValue> Href(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "href");
            return Task.FromResult<FluidValue>(new StringValue(page.Href(input.ToStringValue())));
        }

        public static Task<FluidValue> IsNil(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return Task.FromResult<FluidValue>(new BooleanValue(input is NilValue));
        }

        public static Task<FluidValue> OrDefault(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "or_default");
            var output = page.OrDefault(input.ToObjectValue(), arguments.At(0).ToObjectValue()).ToString();
            return Task.FromResult<FluidValue>(new StringValue(output));
        }

        public static Task<FluidValue> ItemNamed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;
            return Task.FromResult<FluidValue>(new ObjectValue(shape?.Named(arguments.At(0).ToStringValue())));
        }

        public static Task<FluidValue> ItemPrefixed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;

            if (shape != null)
            {
                foreach (var item in shape.Items)
                {
                    var prefixed = item as IShape;

                    if (prefixed != null && prefixed.Metadata.Prefix == arguments.At(0).ToStringValue())
                    {
                        return Task.FromResult<FluidValue>(new ObjectValue(prefixed));
                    }
                }
            }

            return Task.FromResult<FluidValue>(NilValue.Instance);
        }

        public static async Task<FluidValue> ShapeString(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "shape_string");
            return new StringValue((await page.DisplayAsync(input.ToObjectValue())).ToString());
        }

        public static Task<FluidValue> RemoveTags(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var htmlDecode = arguments.HasNamed("html_decode") ? arguments["html_decode"].ToBooleanValue() : false;
            return Task.FromResult<FluidValue>(new StringValue(input.ToStringValue().RemoveTags(htmlDecode)));
        }

        public static async Task<FluidValue> DateTime(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var dateTime = input.ToObjectValue() as DateTime?;

            if (dateTime.HasValue)
            {
                var page = FluidViewTemplate.EnsureFluidPage(context, "date_time");

                Shape shape = arguments.HasNamed("format")
                    ? page.New.DateTime(Utc: dateTime, Format: arguments["format"].ToStringValue())
                    : page.New.DateTime(Utc: dateTime);

                return new StringValue((await page.DisplayAsync(shape)).ToString());
            }

            return input;
        }

        public static Task<FluidValue> PropNamed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return Task.FromResult<FluidValue>(new ObjectValue(((dynamic)input.ToObjectValue())[arguments.At(0).ToStringValue()]));
        }
    }
}