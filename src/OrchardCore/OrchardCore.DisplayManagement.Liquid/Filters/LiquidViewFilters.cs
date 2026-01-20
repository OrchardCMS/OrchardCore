using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Liquid.Filters
{
    public static class LiquidViewFilters
    {
        public static ValueTask<FluidValue> Localize(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var localizer = ctx.GetValue("ViewLocalizer")?.ToObjectValue() as IViewLocalizer;

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

        public static ValueTask<FluidValue> HtmlClass(FluidValue input, FilterArguments _1, TemplateContext _2)
        {
            return new ValueTask<FluidValue>(new StringValue(input.ToStringValue().HtmlClassify()));
        }

        public static ValueTask<FluidValue> ShapeProperties(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (input.ToObjectValue() is IShape shape)
            {
                foreach (var name in arguments.Names)
                {
                    shape.Properties[name.ToPascalCaseUnderscore()] = arguments[name].ToObjectValue();
                }
                return FluidValue.Create(shape, ctx.Options);
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
