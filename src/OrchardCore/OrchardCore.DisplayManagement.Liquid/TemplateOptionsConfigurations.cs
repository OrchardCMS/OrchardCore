using System.Globalization;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Liquid.Values;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid;

public class TemplateOptionsConfigurations : IConfigureOptions<TemplateOptionsBuilder>
{
    private readonly IHostEnvironment _hostEnvironment;

    public TemplateOptionsConfigurations(IHostEnvironment hostEnvironment) => _hostEnvironment = hostEnvironment;

    public void Configure(TemplateOptionsBuilder options)
    {
        options.AddValueConverter(x =>
        {
            if (x is Shape s)
            {
                return ObjectValue.Create(s);
            }
            else if (x is IHtmlContent c)
            {
                return new HtmlContentValue(c);
            }
            else if (x is StringValues sv)
            {
                return new StringValuesValue(sv);
            }

            return null;
        });

        options.WithGlobalValue("Culture", new CultureValue());

        options.WithGlobalValue("Environment", new HostingEnvironmentValue(_hostEnvironment));

        options.WithGlobalValue("Request", new HttpRequestValue());

        options.WithGlobalValue("HttpContext", new HttpContextValue());

        options.WithGlobalValue("TrackingConsent", new TrackingConsentValue());

        options.ConfigureMemberAccess(strategy =>
        {
            strategy.Register<Shape>("*", new ShapeAccessor());
            strategy.Register<ZoneHolding>("*", new ShapeAccessor());
            strategy.Register<ShapeMetadata>();
            strategy.Register<CultureInfo>();

            strategy.Register<FormCollection, FluidValue>((forms, name) =>
            {
                if (name == "Keys")
                {
                    return ArrayValue.Create(forms.Keys.Select(x => StringValue.Create(x)).ToArray());
                }

                return new StringValuesValue(forms[name]);
            });

            strategy.Register<HttpContextItemsWrapper, object>((httpContext, name) => httpContext.Items[name]);
            strategy.Register<QueryCollection, StringValues>((queries, name) => queries[name]);
            strategy.Register<CookieCollectionWrapper, string>((cookies, name) => cookies.RequestCookieCollection[name]);
            strategy.Register<HeaderDictionaryWrapper, StringValues>((headers, name) => headers.HeaderDictionary[name]);
            strategy.Register<RouteValueDictionaryWrapper, object>((headers, name) => headers.RouteValueDictionary[name]);
        });
    }
}
