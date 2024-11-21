using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HostingEnvironmentValue : FluidValue
{
    private readonly IHostEnvironment _hostEnvironment;

    public HostingEnvironmentValue(IHostEnvironment hostEnvironment = null)
        => _hostEnvironment = hostEnvironment;

    public override FluidValues Type => FluidValues.Object;

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

    public override object ToObjectValue() => _hostEnvironment;

    public override string ToStringValue() => _hostEnvironment.EnvironmentName;

    public override async ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(ToStringValue());

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
        => name switch
        {
            "IsDevelopment" => BooleanValue.Create(_hostEnvironment.IsDevelopment()),
            "IsStaging" => BooleanValue.Create(_hostEnvironment.IsStaging()),
            "IsProduction" => BooleanValue.Create(_hostEnvironment.IsProduction()),
            "Name" => StringValue.Create(_hostEnvironment.EnvironmentName),
            _ => NilValue.Instance
        };
}
