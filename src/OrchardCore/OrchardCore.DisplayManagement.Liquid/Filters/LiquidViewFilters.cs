using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public static class LiquidViewFilters
    {
        public static FilterCollection WithLiquidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("t", Localize);
            filters.AddAsyncFilter("html_class", HtmlClass);
            filters.AddAsyncFilter("shape_new", NewShape);
            filters.AddAsyncFilter("shape_render", ShapeRender);
            filters.AddAsyncFilter("shape_stringify", ShapeStringify);

            return filters;
        }

        public static ValueTask<FluidValue> Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
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

            return new ValueTask<FluidValue>(new StringValue(((IViewLocalizer)localizer)
                .GetString(input.ToStringValue(), parameters.ToArray())));
        }

        public static ValueTask<FluidValue> HtmlClass(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return new ValueTask<FluidValue>(new StringValue(input.ToStringValue().HtmlClassify()));
        }

        public static async ValueTask<FluidValue> NewShape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ShapeFactory", out dynamic shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking 'shape_new'");
            }

            var type = input.ToStringValue();
            var properties = new Dictionary<string, object>();

            foreach (var name in arguments.Names)
            {
                properties.Add(LowerKebabToPascalCase(name), arguments[name].ToObjectValue());
            }

            return FluidValue.Create(await ((IShapeFactory)shapeFactory).CreateAsync(type, Arguments.From(properties)));
        }

        public static async ValueTask<FluidValue> ShapeStringify(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_stringify'");
            }

            if (input.ToObjectValue() is IShape shape)
            {
                return new HtmlContentValue(await (Task<IHtmlContent>)displayHelper(shape));
            }

            return NilValue.Instance;
        }

        public static async ValueTask<FluidValue> ShapeRender(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("DisplayHelper", out dynamic displayHelper))
            {
                throw new ArgumentException("DisplayHelper missing while invoking 'shape_render'");
            }

            if (input.ToObjectValue() is IShape shape)
            {
                return new HtmlContentValue(await (Task<IHtmlContent>)displayHelper(shape));
            }

            return NilValue.Instance;
        }

        public static string LowerKebabToPascalCase(string attribute)
        {
            attribute = attribute.Trim();
            var nextIsUpper = true;
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
