using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("t", Localize);
            filters.AddAsyncFilter("href", Href);
            filters.AddAsyncFilter("named", ItemNamed);
            filters.AddAsyncFilter("shape_string", ShapeString);
            filters.AddAsyncFilter("date_time", DateTime);
            filters.AddAsyncFilter("clear_alternates", ClearAlternates);
            filters.AddAsyncFilter("shape_type", ShapeType);
            filters.AddAsyncFilter("display_type", DisplayType);
            filters.AddAsyncFilter("shape_position", ShapePosition);
            filters.AddAsyncFilter("shape_tab", ShapeTab);
            filters.AddAsyncFilter("remove_item", RemoveItem);
            filters.AddAsyncFilter("set_property", SetProperty);

            return filters;
        }

        public static Task<FluidValue> Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "t");
            var localizer = page.GetService<IViewLocalizer>();
            var contextable = localizer as IViewContextAware;

            if (contextable == null)
            {
                return Task.FromResult(input);
            }

            contextable.Contextualize(page.ViewContext);

            var parameters = new List<object>();
            for (var i = 0; i < arguments.Count; i++)
            {
                parameters.Add(arguments.At(i).ToStringValue());
            }

            return Task.FromResult<FluidValue>(new StringValue(localizer.GetString(input.ToStringValue(), parameters.ToArray())));
        }

        public static Task<FluidValue> Href(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "href");
            return Task.FromResult<FluidValue>(new StringValue(page.Href(input.ToStringValue())));
        }

        public static Task<FluidValue> ItemNamed(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;
            return Task.FromResult<FluidValue>(new ObjectValue(shape?.Named(arguments.At(0).ToStringValue())));
        }

        public static async Task<FluidValue> ShapeString(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var page = FluidViewTemplate.EnsureFluidPage(context, "shape_string");
            return new StringValue((await page.DisplayAsync(input.ToObjectValue())).ToString());
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

        public static Task<FluidValue> ClearAlternates(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata.Alternates.Count > 0)
            {
                shape.Metadata.Alternates.Clear();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ShapeType(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.Type = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> DisplayType(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.DisplayType = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ShapePosition(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.Position = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ShapeTab(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as IShape;

            if (shape?.Metadata != null)
            {
                shape.Metadata.Tab = arguments.At(0).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> RemoveItem(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var shape = input.ToObjectValue() as Shape;

            if (shape?.Items != null)
            {
                shape.Remove(arguments.At(0).ToStringValue());
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> SetProperty(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var obj = input.ToObjectValue() as dynamic;

            if (obj != null)
            {
                obj[arguments.At(0).ToStringValue()] = arguments.At(1).ToStringValue();
            }

            return Task.FromResult(input);
        }
    }
}