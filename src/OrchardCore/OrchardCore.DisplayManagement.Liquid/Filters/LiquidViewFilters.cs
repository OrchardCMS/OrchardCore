using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public static class LiquidViewFilters
    {
        public static FilterCollection WithLiquidViewFilters(this FilterCollection filters)
        {
            filters.AddAsyncFilter("t", Localize);
            filters.AddAsyncFilter("html_class", HtmlClass);
            filters.AddAsyncFilter("new_shape", NewShape);
            filters.AddAsyncFilter("shape_string", ShapeString);
            filters.AddAsyncFilter("clear_alternates", ClearAlternates);
            filters.AddAsyncFilter("add_alternates", AddAlternates);
            filters.AddAsyncFilter("clear_classes", ClearClasses);
            filters.AddAsyncFilter("add_classes", AddClasses);
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

        public static Task<FluidValue> HtmlClass(FluidValue input, FilterArguments arguments, TemplateContext context)
        {

            return Task.FromResult<FluidValue>(new StringValue(input.ToStringValue().HtmlClassify()));
        }

        public static async Task<FluidValue> NewShape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (!context.AmbientValues.TryGetValue("ShapeFactory", out dynamic shapeFactory))
            {
                throw new ArgumentException("ShapeFactory missing while invoking 'new_shape'");
            }

            var type = input.ToStringValue();
            var properties = new Dictionary<string, object>();

            foreach (var name in arguments.Names)
            {
                properties.Add(LowerKebabToPascalCase(name), arguments[name].ToObjectValue());
            }

            return FluidValue.Create(await ((IShapeFactory)shapeFactory).CreateAsync(type, Arguments.From(properties)));
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
            if(input.ToObjectValue() is IShape shape && shape.Metadata.Alternates.Count > 0)
            {
                shape.Metadata.Alternates.Clear();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> AddAlternates(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                var alternates = arguments["alternates"].Or(arguments.At(0));

                if (alternates.Type == FluidValues.String)
                {
                    var values = alternates.ToStringValue().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var value in values)
                    {
                        shape.Metadata.Alternates.Add(value);
                    }
                }
                else if (alternates.Type == FluidValues.Array)
                {
                    foreach (var value in alternates.Enumerate())
                    {
                        shape.Metadata.Alternates.Add(value.ToStringValue());
                    }
                }
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ClearClasses(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape && shape.Classes.Count > 0)
            {
                shape.Classes.Clear();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> AddClasses(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                var classes = arguments["classes"].Or(arguments.At(0));

                if (classes.Type == FluidValues.String)
                {
                    var values = classes.ToStringValue().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var value in values)
                    {
                        shape.Classes.Add(value);
                    }
                }
                else if (classes.Type == FluidValues.Array)
                {
                    foreach (var value in classes.Enumerate())
                    {
                        shape.Classes.Add(value.ToStringValue());
                    }
                }
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ShapeType(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                shape.Metadata.Type = arguments["type"].Or(arguments.At(0)).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> DisplayType(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                shape.Metadata.DisplayType = arguments["type"].Or(arguments.At(0)).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ShapePosition(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                shape.Metadata.Position = arguments["position"].Or(arguments.At(0)).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> ShapeTab(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                shape.Metadata.Tab = arguments["tab"].Or(arguments.At(0)).ToStringValue();
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> RemoveItem(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is Shape shape && shape.Items != null)
            {
                shape.Remove(arguments["item"].Or(arguments.At(0)).ToStringValue());
            }

            return Task.FromResult(input);
        }

        public static Task<FluidValue> SetProperties(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var obj = input.ToObjectValue() as dynamic;

            if (obj != null)
            {
                foreach (var name in arguments.Names)
                {
                    var argument = arguments[name];
                    var propertyName = LowerKebabToPascalCase(name);

                    if (argument.Type == FluidValues.Array)
                    {
                        var values = argument.Enumerate();

                        if (values.Count() > 0)
                        {
                            var type = values.First().Type;

                            if (type == FluidValues.String)
                            {
                                obj[propertyName] = values.Select(v => v.ToStringValue());
                            }
                            else if (type == FluidValues.Number)
                            {
                                obj[propertyName] = values.Select(v => v.ToNumberValue());
                            }
                            else if (type == FluidValues.Boolean)
                            {
                                obj[propertyName] = values.Select(v => v.ToBooleanValue());
                            }
                            else
                            {
                                obj[propertyName] = values.Select(v => v.ToObjectValue());
                            }
                        }
                    }
                    else
                    {
                        obj[propertyName] = argument.ToObjectValue();
                    }
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
