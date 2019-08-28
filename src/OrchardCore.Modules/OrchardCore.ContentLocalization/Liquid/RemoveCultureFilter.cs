using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public class RemoveCultureFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var cultureName = arguments["cultureName"]?.ToStringValue();
            if (string.IsNullOrEmpty(cultureName))
            {
                throw new ArgumentException("You must supply a cultureName argument while invoking 'remove_culture'");
            }

            if (input == null || input.Type != FluidValues.Array)
            {
                throw new ArgumentException("Input must be a list of CultureViewModel objects for 'remove_culture'");
            }

            var cultures = input.Enumerate().Select(x=>(CultureViewModel)x.ToObjectValue()).ToList();
            return new ValueTask<FluidValue>(FluidValue.Create(cultures.Where(entry => !string.Equals(entry.Name, cultureName, StringComparison.OrdinalIgnoreCase))));
         }
    }
}
