using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Localization;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class CultureValue : FluidValue
{
    // When null it means this is the global "Culture" object that represents the current UI culture.
    private readonly CultureInfo _culture;

    private CultureInfo Culture => _culture ?? CultureInfo.CurrentUICulture;

    public override FluidValues Type => FluidValues.Object;

    /// <summary>
    /// Creates a new instance of a <see cref="CultureValue"/> that uses <see cref="CultureInfo.CurrentUICulture"/> when resolved.
    /// </summary>
    public CultureValue()
    {
        _culture = null;
    }

    /// <summary>
    /// Creates a new instance of a <see cref="CultureValue"/> for the specified culture.
    /// </summary>
    public CultureValue(CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(culture);

        _culture = culture;
    }

    public override bool Equals(FluidValue other)
    {
        if (other is null)
        {
            return false;
        }

        return ToStringValue() == other.ToStringValue();
    }

    public override bool ToBooleanValue() => true;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => Culture;

    public override string ToStringValue() => Culture.Name;

    public override async ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(Culture.Name);

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        return name switch
        {
            nameof(CultureInfo.Name) => ValueTask.FromResult<FluidValue>(new StringValue(Culture.Name)),
            nameof(CultureInfo.NativeName) => ValueTask.FromResult<FluidValue>(new StringValue(Culture.NativeName)),
            nameof(CultureInfo.DisplayName) => ValueTask.FromResult<FluidValue>(new StringValue(Culture.DisplayName)),
            nameof(CultureInfo.TwoLetterISOLanguageName) => ValueTask.FromResult<FluidValue>(new StringValue(Culture.TwoLetterISOLanguageName)),
            "Dir" => ValueTask.FromResult<FluidValue>(new StringValue(Culture.GetLanguageDirection())),
            "SupportedCultures" => _culture is null ? GetSupportedCulturesAsync(context) : ValueTask.FromResult<FluidValue>(NilValue.Instance),
            "DefaultCulture" => _culture is null ? GetDefaultCultureAsync(context) : ValueTask.FromResult<FluidValue>(NilValue.Instance),
            _ => ValueTask.FromResult<FluidValue>(NilValue.Instance)
        };
    }

    private static async ValueTask<FluidValue> GetSupportedCulturesAsync(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var services = ctx.Services;
        var localizationService = services.GetRequiredService<ILocalizationService>();
        var supportedCultures = await localizationService.GetSupportedCulturesAsync();

        return new ArrayValue(supportedCultures.Select(c => new CultureValue(CultureInfo.GetCultureInfo(c))).ToArray());
    }

    private static async ValueTask<FluidValue> GetDefaultCultureAsync(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var services = ctx.Services;
        var localizationService = services.GetRequiredService<ILocalizationService>();

        return new CultureValue(CultureInfo.GetCultureInfo(await localizationService.GetDefaultCultureAsync()));
    }
}
