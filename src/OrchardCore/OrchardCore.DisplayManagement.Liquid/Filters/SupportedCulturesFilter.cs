using System.Globalization;
using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Localization;

namespace OrchardCore.DisplayManagement.Liquid.Filters;

[Obsolete("This filter is obsolete and will be removed in a future version. Use 'Culture.SupportedCultures' instead.")]
public class SupportedCulturesFilter : ILiquidFilter
{
    private readonly ILocalizationService _localizationService;

    public SupportedCulturesFilter(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
    {
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();

        return new ArrayValue(supportedCultures.Select(x => new ObjectValue(CultureInfo.GetCultureInfo(x))).ToArray());
    }
}
