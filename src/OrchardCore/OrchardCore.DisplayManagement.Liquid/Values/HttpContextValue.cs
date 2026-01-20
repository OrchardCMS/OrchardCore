using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HttpContextValue : FluidValue
{
    public override FluidValues Type => FluidValues.Object;

    public override bool Equals(FluidValue other)
    {
        if (other is null)
        {
            return false;
        }

        return other is HttpContextValue;
    }

    public override bool ToBooleanValue() => true;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => null;

    public override string ToStringValue() => "HttpContext";

    public override async ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(ToStringValue());

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var httpContext = GetHttpContext(context);

        if (httpContext is null)
        {
            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        return name switch
        {
            nameof(HttpContext.Items) => new ObjectValue(new HttpContextItemsWrapper(httpContext.Items)),
            _ => NilValue.Instance
        };
    }

    private static HttpContext GetHttpContext(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var httpContextAccessor = ctx.Services.GetRequiredService<IHttpContextAccessor>();

        return httpContextAccessor.HttpContext;
    }
}
