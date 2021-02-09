using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid
{
    public static class ContentLocalizationFilter
    {
        public static async ValueTask<FluidValue> LocalizationSet(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var innoFieldsService = context.Services.GetRequiredService<IContentLocalizationManager>();

            var locale = arguments.At(0).ToStringValue();

            if (arguments.At(0).IsNil())
            {
                locale = ctx.CultureInfo.Name;
            }

            if (input.Type == FluidValues.Array)
            {
                // List of content item ids

                var localizationSets = input.Enumerate().Select(x => x.ToStringValue()).ToArray();

                return FluidValue.Create(await innoFieldsService.GetItemsForSetsAsync(localizationSets, locale), ctx.Options);
            }
            else
            {
                var localizationSet = input.ToStringValue();

                return FluidValue.Create(await innoFieldsService.GetContentItemAsync(localizationSet, locale), ctx.Options);
            }
        }
    }
}
