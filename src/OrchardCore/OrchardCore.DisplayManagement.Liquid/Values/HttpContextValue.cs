using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed partial class HttpContextValue : FluidValue
{
    private readonly HttpContext _context;

    public override FluidValues Type => FluidValues.Object;

    /// <summary>
    /// Creates a new instance of a <see cref="HttpContextValue"/> for the specified HTTP context.
    /// </summary>
    public HttpContextValue(HttpContext context = null)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public override bool Equals(FluidValue other)
    {
        if (other is null)
        {
            return false;
        }

        return ToObjectValue() == other.ToObjectValue();
    }

    public override bool ToBooleanValue() => true;

    public override decimal ToNumberValue() => 0;

    public override object ToObjectValue() => _context;

    public override string ToStringValue() => _context?.ToString();

#pragma warning disable CS0672 // Member overrides obsolete member
    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
#pragma warning restore CS0672 // Member overrides obsolete member
        => writer.Write(_context?.ToString());

    public async override ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(_context?.ToString());

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var httpContext = _context ?? GetHttpContext(context).ToObjectValue() as HttpContext;

        return name switch
        {
            nameof(HttpContext.Items) => new ObjectValue(new HttpContextItemsWrapper(httpContext.Items)),
            _ => NilValue.Instance
        };
    }

    private static ObjectValue GetHttpContext(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var httpContextAccessor = ctx.Services.GetRequiredService<IHttpContextAccessor>();

        return new ObjectValue(httpContextAccessor.HttpContext);
    }
}
