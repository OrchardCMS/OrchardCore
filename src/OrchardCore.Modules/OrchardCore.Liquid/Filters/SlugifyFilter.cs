using Fluid;
using Fluid.Values;
using OrchardCore.Localization;
using OrchardCore.Modules.Services;

namespace OrchardCore.Liquid.Filters;

public class SlugifyFilter : ILiquidFilter
{
    private readonly ISlugService _slugService;

    public SlugifyFilter(ISlugService slugService)
    {
        _slugService = slugService;
    }
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        var transliterateArg = arguments["transliterate"];
        var transliterate = transliterateArg.IsNil() || transliterateArg.ToBooleanValue();
        var slug = transliterate
            ? _slugService.SlugifyWithTransliteration(input.ToStringValue())
            : _slugService.Slugify(input.ToStringValue());

        return new StringValue(slug);
    }
}
