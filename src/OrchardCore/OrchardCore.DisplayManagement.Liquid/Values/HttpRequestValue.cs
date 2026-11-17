using System.Text.Encodings.Web;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid.Values;

internal sealed class HttpRequestValue : FluidValue
{
    public override FluidValues Type => FluidValues.Object;

    public override bool Equals(FluidValue other)
    {
        if (other is null)
        {
            return false;
        }

        return other is HttpRequestValue;
    }

    public override bool ToBooleanValue(TemplateContext context) => true;

    public override decimal ToNumberValue(TemplateContext context) => 0;

    public override object ToObjectValue(TemplateContext context) => null;

    public override string ToStringValue(TemplateContext context) => "Request";

    public override ValueTask WriteToAsync(IFluidOutput output, TextEncoder encoder, TemplateContext context)
    {
        output.Write(ToStringValue(context));

        return ValueTask.CompletedTask;
    }

    public override ValueTask<FluidValue> GetValueAsync(string name, TemplateContext context)
    {
        var request = GetHttpRequest(context);

        if (request is null)
        {
            return new ValueTask<FluidValue>(NilValue.Instance);
        }

        return name switch
        {
            nameof(HttpRequest.QueryString) => StringValue.Create(request.QueryString.Value ?? string.Empty),
            nameof(HttpRequest.ContentType) => StringValue.Create(request.ContentType ?? string.Empty),
            nameof(HttpRequest.ContentLength) => NumberValue.Create(request.ContentLength ?? 0),
            nameof(HttpRequest.Cookies) => ObjectValue.Create(new CookieCollectionWrapper(request.Cookies)),
            nameof(HttpRequest.Headers) => ObjectValue.Create(new HeaderDictionaryWrapper(request.Headers)),
            nameof(HttpRequest.Query) => ObjectValue.Create(new QueryCollection(request.Query.ToDictionary(kv => kv.Key, kv => kv.Value))),
            nameof(HttpRequest.Form) => request.HasFormContentType ? (FluidValue)ObjectValue.Create(request.Form) : NilValue.Instance,
            nameof(HttpRequest.Protocol) => StringValue.Create(request.Protocol ?? string.Empty),
            nameof(HttpRequest.Path) => StringValue.Create(request.Path.Value ?? string.Empty),
            nameof(HttpRequest.PathBase) => StringValue.Create(request.PathBase.Value ?? string.Empty),
            nameof(HttpRequest.Host) => StringValue.Create(request.Host.Value ?? string.Empty),
            nameof(HttpRequest.IsHttps) => BooleanValue.Create(request.IsHttps),
            nameof(HttpRequest.Scheme) => StringValue.Create(request.Scheme ?? string.Empty),
            nameof(HttpRequest.Method) => StringValue.Create(request.Method ?? string.Empty),
            nameof(HttpRequest.RouteValues) => ObjectValue.Create(new RouteValueDictionaryWrapper(request.RouteValues)),

            // Provides correct escaping to reconstruct a request or redirect URI.
            "UriHost" => StringValue.Create(request.Host.ToUriComponent(), encode: false),
            "UriPath" => StringValue.Create(request.Path.ToUriComponent(), encode: false),
            "UriPathBase" => StringValue.Create(request.PathBase.ToUriComponent(), encode: false),
            "UriQueryString" => StringValue.Create(request.QueryString.ToUriComponent(), encode: false),
            _ => ValueTask.FromResult<FluidValue>(NilValue.Instance)
        };
    }

    private static HttpRequest GetHttpRequest(TemplateContext context)
    {
        var ctx = context as LiquidTemplateContext
            ?? throw new InvalidOperationException($"An implementation of '{nameof(LiquidTemplateContext)}' is required");

        var httpContext = ctx.Services.GetRequiredService<IHttpContextAccessor>().HttpContext;

        return httpContext.Request;
    }
}
