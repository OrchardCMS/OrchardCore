using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors.ShapeTemplateStrategy;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.DisplayManagement.Liquid.Filters;
using OrchardCore.DisplayManagement.Liquid.TagHelpers;
using OrchardCore.DisplayManagement.Liquid.Tags;
using OrchardCore.DisplayManagement.Razor;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Liquid;
using OrchardCore.Localization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level services for managing liquid view template files.
        /// </summary>
        public static OrchardCoreBuilder AddLiquidViews(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<LiquidViewParser>();
                services.AddSingleton<IAnchorTag, AnchorTag>();

                services.AddTransient<IConfigureOptions<TemplateOptions>, TemplateOptionsFileProviderSetup>();

                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<LiquidViewOptions>,
                    LiquidViewOptionsSetup>());

                services.TryAddEnumerable(
                    ServiceDescriptor.Transient<IConfigureOptions<ShapeTemplateOptions>,
                    LiquidShapeTemplateOptionsSetup>());

                services.AddSingleton<IApplicationFeatureProvider<ViewsFeature>, LiquidViewsFeatureProvider>();
                services.AddScoped<IRazorViewExtensionProvider, LiquidViewExtensionProvider>();
                services.AddSingleton<LiquidTagHelperFactory>();

                services.AddScoped<ILiquidTemplateEventHandler, RequestLiquidTemplateEventHandler>();
                services.AddScoped<ILiquidTemplateEventHandler, CultureLiquidTemplateEventHandler>();

                services.Configure<TemplateOptions>(o =>
                {
                    o.MemberAccessStrategy.Register<CultureInfo, FluidValue>((culture, name) =>
                    {
                        return name switch
                        {
                            nameof(CultureInfo.Name) => new StringValue(culture.Name),
                            "Dir" => new StringValue(culture.GetLanguageDirection()),
                            _ => null
                        };
                    });

                    o.ValueConverters.Add(o => o is Shape s ? new ObjectValue(s) : null);
                    o.ValueConverters.Add(o => o is ZoneHolding z ? new ObjectValue(z) : null);
                    o.ValueConverters.Add(o => !(o is IShape) && o is IHtmlContent c ? new HtmlContentValue(c) : null);

                    o.MemberAccessStrategy.Register<Shape>("*", new ShapeAccessor());
                    o.MemberAccessStrategy.Register<ZoneHolding>("*", new ShapeAccessor());
                    o.MemberAccessStrategy.Register<ShapeMetadata>();

                    o.MemberAccessStrategy.Register<HttpRequest, FluidValue>((request, name) =>
                    {
                        switch (name)
                        {
                            case "QueryString": return new StringValue(request.QueryString.Value);
                            case "ContentType": return new StringValue(request.ContentType);
                            case "ContentLength": return NumberValue.Create(request.ContentLength ?? 0);
                            case "Cookies": return new ObjectValue(new CookieCollectionWrapper(request.Cookies));
                            case "Headers": return new ObjectValue(new HeaderDictionaryWrapper(request.Headers));
                            case "Query": return new ObjectValue(request.Query);
                            case "Form": return request.HasFormContentType ? (FluidValue)new ObjectValue(request.Form) : NilValue.Instance;
                            case "Protocol": return new StringValue(request.Protocol);
                            case "Path": return new StringValue(request.Path.Value);
                            case "PathBase": return new StringValue(request.PathBase.Value);
                            case "Host": return new StringValue(request.Host.Value);
                            case "IsHttps": return BooleanValue.Create(request.IsHttps);
                            case "Scheme": return new StringValue(request.Scheme);
                            case "Method": return new StringValue(request.Method);
                            case "Route": return new ObjectValue(new RouteValueDictionaryWrapper(request.RouteValues));

                            default: return null;
                        }
                    });

                    o.MemberAccessStrategy.Register<FormCollection, FluidValue>((forms, name) =>
                    {
                        if (name == "Keys")
                        {
                            return new ArrayValue(forms.Keys.Select(x => new StringValue(x)));
                        }

                        return new ArrayValue(forms[name].Select(x => new StringValue(x)).ToArray());
                    });

                    o.MemberAccessStrategy.Register<HttpContext, FluidValue>((httpcontext, name) =>
                    {
                        switch (name)
                        {
                            case "Items": return new ObjectValue(new HttpContextItemsWrapper(httpcontext.Items));
                            default: return null;
                        }
                    });

                    o.MemberAccessStrategy.Register<HttpContextItemsWrapper, object>((httpContext, name) => httpContext.Items[name]);
                    o.MemberAccessStrategy.Register<QueryCollection, string[]>((queries, name) => queries[name].ToArray());
                    o.MemberAccessStrategy.Register<CookieCollectionWrapper, string>((cookies, name) => cookies.RequestCookieCollection[name]);
                    o.MemberAccessStrategy.Register<HeaderDictionaryWrapper, string[]>((headers, name) => headers.HeaderDictionary[name].ToArray());
                    o.MemberAccessStrategy.Register<RouteValueDictionaryWrapper, object>((headers, name) => headers.RouteValueDictionary[name]);

                })
                .AddLiquidFilter<AppendVersionFilter>("append_version")
                .AddLiquidFilter<ResourceUrlFilter>("resource_url")
                .AddLiquidFilter<SanitizeHtmlFilter>("sanitize_html");
            });

            return builder;
        }

        private class CookieCollectionWrapper
        {
            public readonly IRequestCookieCollection RequestCookieCollection;

            public CookieCollectionWrapper(IRequestCookieCollection requestCookieCollection)
            {
                RequestCookieCollection = requestCookieCollection;
            }
        }

        private class HeaderDictionaryWrapper
        {
            public readonly IHeaderDictionary HeaderDictionary;

            public HeaderDictionaryWrapper(IHeaderDictionary headerDictionary)
            {
                HeaderDictionary = headerDictionary;
            }
        }

        private class HttpContextItemsWrapper
        {
            public readonly IDictionary<object, object> Items;

            public HttpContextItemsWrapper(IDictionary<object, object> items)
            {
                Items = items;
            }
        }

        private class RouteValueDictionaryWrapper
        {
            public readonly IReadOnlyDictionary<string, object> RouteValueDictionary;

            public RouteValueDictionaryWrapper(IReadOnlyDictionary<string, object> routeValueDictionary)
            {
                RouteValueDictionary = routeValueDictionary;
            }
        }
    }
}
