using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Shortcodes;
using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Filters
{
    public class ShortcodeArgumentNamedOrDefaultFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var shortcodeArguments = input.ToObjectValue() as Arguments;



            var namedArgument = arguments.At(0).ToStringValue();
            if (String.IsNullOrEmpty(namedArgument))
            {
                namedArgument = "0";

            }
            var result = shortcodeArguments.NamedOrDefault(namedArgument);

            return new ValueTask<FluidValue>(new StringValue(result));


        }
    }
}
