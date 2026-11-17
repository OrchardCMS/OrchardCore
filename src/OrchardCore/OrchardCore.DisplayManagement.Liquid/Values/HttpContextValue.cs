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

    public override bool ToBooleanValue(TemplateContext context) => true;

    public override decimal ToNumberValue(TemplateContext context) => 0;

    public override object ToObjectValue(TemplateContext context) => null;

    public override string ToStringValue(TemplateContext context) => "HttpContext";

    public override ValueTask WriteToAsync(IFluidOutput output, TextEncoder encoder, TemplateContext context)
    {
        output.Write(ToStringValue(context));

        return ValueTask.CompletedTask;
    }

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var httpContext = GetHttpContext(context);

        if (httpContext is null)
        {
            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        return name switch
        {
            nameof(HttpContext.Items) => ObjectValue.Create(new HttpContextItemsWrapper(httpContext.Items)),
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
