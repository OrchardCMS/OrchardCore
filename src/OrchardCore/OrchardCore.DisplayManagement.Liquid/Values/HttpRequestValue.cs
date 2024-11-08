using System.Globalization;
using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HttpRequestValue : FluidValue
{
    private readonly HttpRequest _request;

    public override FluidValues Type => FluidValues.Object;

    /// <summary>
    /// Creates a new instance of a <see cref="HttpRequestValue"/> for the specified HTTP request.
    /// </summary>
    public HttpRequestValue(HttpRequest request = null)
        => _request = request;

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

    public override object ToObjectValue() => _request;

    public override string ToStringValue() => _request?.Path;

#pragma warning disable CS0672 // Member overrides obsolete member
    public override void WriteTo(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
#pragma warning restore CS0672 // Member overrides obsolete member
        => writer.Write(_request?.Path);

    public async override ValueTask WriteToAsync(TextWriter writer, TextEncoder encoder, CultureInfo cultureInfo)
        => await writer.WriteAsync(_request?.Path);

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var request = _request ?? GetHttpRequest(context).ToObjectValue() as HttpRequest;

        return name switch
        {
            nameof(HttpRequest.QueryString) => new StringValue(request.QueryString.Value),
            nameof(HttpRequest.ContentType) => new StringValue(request.ContentType),
            nameof(HttpRequest.ContentLength) => NumberValue.Create(request.ContentLength ?? 0),
            nameof(HttpRequest.Cookies) => new ObjectValue(new CookieCollectionWrapper(request.Cookies)),
            nameof(HttpRequest.Headers) => new ObjectValue(new HeaderDictionaryWrapper(request.Headers)),
            nameof(HttpRequest.Query) => new ObjectValue(new QueryCollection(request.Query.ToDictionary(kv => kv.Key, kv => kv.Value))),
            nameof(HttpRequest.Form) => request.HasFormContentType ? (FluidValue)new ObjectValue(request.Form) : NilValue.Instance,
            nameof(HttpRequest.Protocol) => new StringValue(request.Protocol),
            nameof(HttpRequest.Path) => new StringValue(request.Path.Value),
            nameof(HttpRequest.PathBase) => new StringValue(request.PathBase.Value),
            nameof(HttpRequest.Host) => new StringValue(request.Host.Value),
            nameof(HttpRequest.IsHttps) => BooleanValue.Create(request.IsHttps),
            nameof(HttpRequest.Scheme) => new StringValue(request.Scheme),
            nameof(HttpRequest.Method) => new StringValue(request.Method),
            nameof(HttpRequest.RouteValues) => new ObjectValue(new RouteValueDictionaryWrapper(request.RouteValues)),

            // Provides correct escaping to reconstruct a request or redirect URI.
            "UriHost" => new StringValue(request.Host.ToUriComponent(), encode: false),
            "UriPath" => new StringValue(request.Path.ToUriComponent(), encode: false),
            "UriPathBase" => new StringValue(request.PathBase.ToUriComponent(), encode: false),
            "UriQueryString" => new StringValue(request.QueryString.ToUriComponent(), encode: false),
            _ => ValueTask.FromResult<FluidValue>(NilValue.Instance)
        };
    }

    private static ObjectValue GetHttpRequest(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var httpContext = ctx.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;

        return new ObjectValue(httpContext.Request);
    }
}
