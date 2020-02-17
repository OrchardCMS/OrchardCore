using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public class ContentLocalizationFilter : ILiquidFilter
    {
        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            if (!ctx.AmbientValues.TryGetValue("Services", out var services))
            {
                throw new ArgumentException("Services missing while invoking 'localization_set'");
            }

            var innoFieldsService = ((IServiceProvider)services).GetRequiredService<IContentLocalizationManager>();

            var locale = arguments.At(0).ToStringValue();

            if (arguments.At(0).IsNil())
            {
                locale = ctx.CultureInfo.Name;
            }

            if (input.Type == FluidValues.Array)
            {
                // List of content item ids

                var localizationSets = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await innoFieldsService.GetItemsForSetsAsync(localizationSets, locale));
            }
            else
            {
                var localizationSet = input.ToStringValue();

                return FluidValue.Create(await innoFieldsService.GetContentItemAsync(localizationSet, locale));
            }
        }
    }
}
