using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HostingEnvironmentValue : FluidValue
{
    private readonly string _environmentName;

    public HostingEnvironmentValue(string environmentName = null)
        => _environmentName = environmentName;

    public override FluidValues Type => FluidValues.String;

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

    public override object ToObjectValue() => _environmentName;

    public override string ToStringValue() => _environmentName;

#pragma warning disable CS0672 // Member overrides obsolete member
    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
#pragma warning restore CS0672 // Member overrides obsolete member
        => writer.Write(_environmentName);

    public async override ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(_environmentName);

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var environmentName = _environmentName ?? GetHostingEnvironment(context).ToStringValue();

        return name switch
        {
            "IsDevelopment" => BooleanValue.Create(environmentName == Environments.Development),
            "IsStaging" => BooleanValue.Create(environmentName == Environments.Staging),
            "IsProduction" => BooleanValue.Create(environmentName == Environments.Production),
            "Name" => StringValue.Create(environmentName),
            _ => NilValue.Instance
        };
    }

    private static HostingEnvironmentValue GetHostingEnvironment(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var hostEnvironment = ctx.Services.GetRequiredService<IHostEnvironment>();

        return new HostingEnvironmentValue(hostEnvironment.EnvironmentName);
    }
}
