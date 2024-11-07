using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using OrchardCore.Localization;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class CultureValue : FluidValue
{
    public override FluidValues Type => FluidValues.Object;

    public override bool Equals(FluidValue other)
    {
        if (other is null)
        {
            return false;
        }

        return ToStringValue() == other.ToStringValue();
    }

    public override bool ToBooleanValue() => false;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => ToStringValue();

    public override string ToStringValue() => _culture.Name;

#pragma warning disable CS0672 // Member overrides obsolete member
    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
#pragma warning restore CS0672 // Member overrides obsolete member
        => writer.Write(ToStringValue());

    public async override ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(ToStringValue());

    protected override FluidValue GetValue(string name, TemplateContext context)
        => name switch
        {
            nameof(CultureInfo.Name) => new StringValue(CultureInfo.CurrentUICulture.Name),
            "Dir" => new StringValue(CultureInfo.CurrentUICulture.GetLanguageDirection()),
            nameof(CultureInfo.NativeName) => new StringValue(CultureInfo.CurrentUICulture.NativeName),
            nameof(CultureInfo.DisplayName) => new StringValue(CultureInfo.CurrentUICulture.DisplayName),
            nameof(CultureInfo.TwoLetterISOLanguageName) => new StringValue(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName),
            _ => NilValue.Instance
        };
}
