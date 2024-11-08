using System.Globalization;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid.Values;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid;

public class TemplateOptionsConfigurations : IConfigureOptions<TemplateOptions>
{
    private readonly IHostEnvironment _hostEnvironment;

    public TemplateOptionsConfigurations(IHostEnvironment hostEnvironment) => _hostEnvironment = hostEnvironment;

    public void Configure(TemplateOptions options)
    {
        options.ValueConverters.Add(x =>
        {
            if (x is Shape s)
            {
                return new ObjectValue(s);
            }
            else if (x is IHtmlContent c)
            {
                return new HtmlContentValue(c);
            }

            return null;
        });

        options.MemberAccessStrategy.Register<Shape>("*", new ShapeAccessor());
        options.MemberAccessStrategy.Register<ZoneHolding>("*", new ShapeAccessor());
        options.MemberAccessStrategy.Register<ShapeMetadata>();
        options.MemberAccessStrategy.Register<CultureInfo>();

        options.Scope.SetValue("Culture", new CultureValue());

        options.Scope.SetValue("Environment", new HostingEnvironmentValue(_hostEnvironment));

        options.Scope.SetValue("Request", new ObjectValue(new LiquidRequestAccessor()));
        options.MemberAccessStrategy.Register<LiquidRequestAccessor, FluidValue>((obj, name, ctx) =>
        {
            var request = ((LiquidTemplateContext)ctx).Services.GetRequiredService<IHttpContextAccessor>().HttpContext?.Request;
            if (request != null)
            {
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

                    _ => NilValue.Instance
                };
            }

            return NilValue.Instance;
        });

        options.Scope.SetValue("HttpContext", new ObjectValue(new LiquidHttpContextAccessor()));
        options.MemberAccessStrategy.Register<LiquidHttpContextAccessor, FluidValue>((obj, name, ctx) =>
        {
            var httpContext = ((LiquidTemplateContext)ctx).Services.GetRequiredService<IHttpContextAccessor>().HttpContext;
            if (httpContext != null)
            {
                return name switch
                {
                    nameof(HttpContext.Items) => new ObjectValue(new HttpContextItemsWrapper(httpContext.Items)),
                    _ => NilValue.Instance
                };
            }

            return NilValue.Instance;
        });

        options.MemberAccessStrategy.Register<FormCollection, FluidValue>((forms, name) =>
        {
            if (name == "Keys")
            {
                return new ArrayValue(forms.Keys.Select(x => new StringValue(x)).ToArray());
            }

            return new ArrayValue(forms[name].Select(x => new StringValue(x)).ToArray());
        });

        options.MemberAccessStrategy.Register<HttpContextItemsWrapper, object>((httpContext, name) => httpContext.Items[name]);
        options.MemberAccessStrategy.Register<QueryCollection, string[]>((queries, name) => queries[name].ToArray());
        options.MemberAccessStrategy.Register<CookieCollectionWrapper, string>((cookies, name) => cookies.RequestCookieCollection[name]);
        options.MemberAccessStrategy.Register<HeaderDictionaryWrapper, string[]>((headers, name) => headers.HeaderDictionary[name].ToArray());
        options.MemberAccessStrategy.Register<RouteValueDictionaryWrapper, object>((headers, name) => headers.RouteValueDictionary[name]);
    }

    private sealed class CookieCollectionWrapper
    {
        public readonly IRequestCookieCollection RequestCookieCollection;

        public CookieCollectionWrapper(IRequestCookieCollection requestCookieCollection)
        {
            RequestCookieCollection = requestCookieCollection;
        }
    }

    private sealed class HeaderDictionaryWrapper
    {
        public readonly IHeaderDictionary HeaderDictionary;

        public HeaderDictionaryWrapper(IHeaderDictionary headerDictionary)
        {
            HeaderDictionary = headerDictionary;
        }
    }

    private sealed class HttpContextItemsWrapper
    {
        public readonly IDictionary<object, object> Items;

        public HttpContextItemsWrapper(IDictionary<object, object> items)
        {
            Items = items;
        }
    }

    private sealed class RouteValueDictionaryWrapper
    {
        public readonly IReadOnlyDictionary<string, object> RouteValueDictionary;

        public RouteValueDictionaryWrapper(IReadOnlyDictionary<string, object> routeValueDictionary)
        {
            RouteValueDictionary = routeValueDictionary;
        }
    }
}
