using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Fluid.Filters
{
    public static class FluidViewFilters
    {
        public static FilterCollection WithFluidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("t", Localize);
            filters.AddAsyncFilter("date_time", DateTimeShape);
            filters.AddAsyncFilter("shape_string", ShapeString);
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
            if (!context.AmbientValues.TryGetValue("ViewLocalizer", out var localizer))
            {
                throw new ArgumentException("ViewLocalizer missing while invoking 't'");
            }

            var parameters = new List<object>();
            for (var i = 0; i < arguments.Count; i++)
            {
                parameters.Add(arguments.At(i).ToStringValue());
            }

            return Task.FromResult<FluidValue>(new StringValue(((IViewLocalizer)localizer)
                .GetString(input.ToStringValue(), parameters.ToArray())));
        }

        public static async Task<FluidValue> DateTimeShape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ShapeFactory", out dynamic shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking 'date_time'");
            }

            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'date_time'");
            }

            var obj = input.ToObjectValue();
            DateTime? dateTime = null;

            if (obj is string stringDate)
            {
                var date = DateTime.Parse(stringDate, context.CultureInfo, DateTimeStyles.AssumeUniversal);
                dateTime = new DateTime?(date);
            }
            else if (obj is DateTime date)
            {
                dateTime = new DateTime?(date);
            }
            else if (obj is DateTimeOffset dateTimeOffset)
            {
                dateTime = new DateTime?(dateTimeOffset.Date);
            }
            if (dateTime.HasValue)
            {
                Shape shape = arguments.HasNamed("format")
                    ? shapeFactory.DateTime(Utc: dateTime, Format: arguments["format"].ToStringValue())
                    : shapeFactory.DateTime(Utc: dateTime);

                return new StringValue((await (Task<IHtmlContent>)displayHelper(shape)).ToString());
            }

            return input;
        }

        public static async Task<FluidValue> ShapeString(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_string'");
            }

            var shape = input.ToObjectValue() as IShape;
            return new StringValue((await (Task<IHtmlContent>)displayHelper(shape)).ToString());
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