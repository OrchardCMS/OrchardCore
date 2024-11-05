using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using OrchardCore.Localization;

namespace OrchardCore.DisplayManagement.Liquid.Values;

public class CultureValue : FluidValue
{
    private readonly CultureInfo _culture;

    public CultureValue(CultureInfo culture) => _culture = culture;

    public override FluidValues Type => FluidValues.Object;

    public override bool Equals(FluidValue other)
    {
        throw new NotImplementedException();
    }

    public override bool ToBooleanValue() => false;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => ToStringValue();

    public override string ToStringValue() => _culture.Name;

    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => writer.Write(ToStringValue());

    protected override FluidValue GetValue(string name, TemplateContext context)
        => name switch
        {
            nameof(CultureInfo.Name) => new StringValue(_culture.Name),
            "Dir" => new StringValue(_culture.GetLanguageDirection()),
            nameof(CultureInfo.NativeName) => new StringValue(_culture.NativeName),
            nameof(CultureInfo.DisplayName) => new StringValue(_culture.DisplayName),
            nameof(CultureInfo.TwoLetterISOLanguageName) => new StringValue(_culture.TwoLetterISOLanguageName),
            _ => NilValue.Instance
        };
}
