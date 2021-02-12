using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public static class LiquidViewFilters
    {
        public static ValueTask<FluidValue> Localize(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var localizer = context.GetValue("ViewLocalizer")?.ToObjectValue() as IViewLocalizer;

            if (localizer == null)
            {
                return ThrowArgumentException<ValueTask<FluidValue>>("ViewLocalizer missing while invoking 't'");
            }

            var parameters = new object[arguments.Count];
            for (var i = 0; i < arguments.Count; i++)
            {
                parameters[i] = arguments.At(i).ToStringValue();
            }

            return new ValueTask<FluidValue>(new StringValue(localizer.GetString(input.ToStringValue(), parameters)));
        }

        public static ValueTask<FluidValue> HtmlClass(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            return new ValueTask<FluidValue>(new StringValue(input.ToStringValue().HtmlClassify()));
        }

        public static ValueTask<FluidValue> NewShape(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            static async ValueTask<FluidValue> Awaited(ValueTask<IShape> task, TemplateOptions options)
            {
                return FluidValue.Create(await task, options);
            }


            var services = ((LiquidTemplateContext)context).Services;

            var shapeFactory = services.GetRequiredService<IShapeFactory>();

            var type = input.ToStringValue();
            var properties = new Dictionary<string, object>(arguments.Count);

            foreach (var name in arguments.Names)
            {
                properties.Add(name.ToPascalCaseUnderscore(), arguments[name].ToObjectValue());
            }

            var task = shapeFactory.CreateAsync(type, Arguments.From(properties));
            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(task, context.Options);
            }
            return new ValueTask<FluidValue>(FluidValue.Create(task.Result, context.Options));
        }

        public static ValueTask<FluidValue> ShapeStringify(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                return new HtmlContentValue(await task);
            }

            if (input.ToObjectValue() is IShape shape)
            {

                var services = ((LiquidTemplateContext)context).Services;

                var displayHelper = services.GetRequiredService<IDisplayHelper>();

                var task = displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }

                StringValue value;
                using (var writer = new StringWriter())
                {
                    task.Result.WriteTo(writer, NullHtmlEncoder.Default);
                    value = new StringValue(writer.ToString(), false);
                }

                return new ValueTask<FluidValue>(value);
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        public static ValueTask<FluidValue> ShapeRender(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            static async ValueTask<FluidValue> Awaited(Task<IHtmlContent> task)
            {
                return new HtmlContentValue(await task);
            }

            var shape = input.ToObjectValue();
            if (shape != null)
            {
                var services = ((LiquidTemplateContext)context).Services;

                var displayHelper = services.GetRequiredService<IDisplayHelper>();

                var task = displayHelper.ShapeExecuteAsync(shape);
                if (!task.IsCompletedSuccessfully)
                {
                    return Awaited(task);
                }
                return new ValueTask<FluidValue>(new HtmlContentValue(task.Result));
            }

            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        public static ValueTask<FluidValue> ShapeProperties(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                foreach (var name in arguments.Names)
                {
                    shape.Properties[name.ToPascalCaseUnderscore()] = arguments[name].ToObjectValue();
                }
                return FluidValue.Create(shape, context.Options);
            }

            return NilValue.Instance;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static T ThrowArgumentException<T>(string message)
        {
            throw new ArgumentException(message);
        }
    }
}
