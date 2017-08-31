using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Orchard.DisplayManagement.Shapes;

namespace Orchard.DisplayManagement.Liquid.Filters
{
    public static class LiquidViewFilters
    {
        public static FilterCollection WithLiquidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("t", Localize);
            filters.AddAsyncFilter("new_shape", NewShape);
            filters.AddAsyncFilter("shape_string", ShapeString);
            filters.AddAsyncFilter("clear_alternates", ClearAlternates);
            filters.AddAsyncFilter("shape_type", ShapeType);
            filters.AddAsyncFilter("display_type", DisplayType);
            filters.AddAsyncFilter("shape_position", ShapePosition);
            filters.AddAsyncFilter("shape_tab", ShapeTab);
            filters.AddAsyncFilter("remove_item", RemoveItem);
            filters.AddAsyncFilter("set_properties", SetProperties);

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

        public static Task<FluidValue> NewShape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ShapeFactory", out dynamic shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking 'date_time'");
            }

            var type = input.ToStringValue();
            var properties = new Dictionary<string, object>();

            foreach (var name in arguments.Names)
            {
                properties.Add(LowerKebabToPascalCase(name), arguments[name].ToObjectValue());
            }

            return Task.FromResult(FluidValue.Create(((IShapeFactory)shapeFactory)
                .Create(type, Arguments.From(properties))));
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

        public static Task<FluidValue> SetProperties(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var obj = input.ToObjectValue() as dynamic;

            if (obj != null)
            {
                var properties = new Dictionary<string, object>();

                foreach (var name in arguments.Names)
                {
                    obj[LowerKebabToPascalCase(name)] = arguments[name].ToObjectValue();
                }
            }

            return Task.FromResult(input);
        }

        public static string LowerKebabToPascalCase(string attribute)
        {
            attribute = attribute.Trim();
            bool nextIsUpper = true;
            var result = new StringBuilder();
            for (int i = 0; i < attribute.Length; i++)
            {
                var c = attribute[i];
                if (c == '_')
                {
                    nextIsUpper = true;
                    continue;
                }

                if (nextIsUpper)
                {
                    result.Append(c.ToString().ToUpper());
                }
                else
                {
                    result.Append(c);
                }

                nextIsUpper = false;
            }

            return result.ToString();
        }
    }
}