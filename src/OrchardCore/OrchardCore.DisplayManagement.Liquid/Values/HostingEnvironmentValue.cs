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

    public override bool ToBooleanValue() => false;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => _hostEnvironment;

    public override string ToStringValue() => _hostEnvironment.EnvironmentName;

#pragma warning disable CS0672 // Member overrides obsolete member
    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
#pragma warning restore CS0672 // Member overrides obsolete member
        => writer.Write(_hostEnvironment.EnvironmentName);

    public async override ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(_hostEnvironment.EnvironmentName);

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var hostingEnvironment = _hostEnvironment ?? GetHostingEnvironment(context).ToObjectValue() as IHostEnvironment;

        return name switch
        {
            "IsDevelopment" => BooleanValue.Create(hostingEnvironment.IsDevelopment()),
            "IsStaging" => BooleanValue.Create(hostingEnvironment.IsStaging()),
            "IsProduction" => BooleanValue.Create(hostingEnvironment.IsProduction()),
            "Name" => StringValue.Create(hostingEnvironment.EnvironmentName),
            _ => NilValue.Instance
        };
    }

    private static HostingEnvironmentValue GetHostingEnvironment(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var hostEnvironment = ctx.Services.GetRequiredService<IHostEnvironment>();

        return new HostingEnvironmentValue(hostEnvironment);
    }
}
