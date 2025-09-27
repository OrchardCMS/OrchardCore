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
            else if (x is StringValues sv)
            {
                return new StringValuesValue(sv);
            }

            return null;
        });

        options.MemberAccessStrategy.Register<Shape>("*", new ShapeAccessor());
        options.MemberAccessStrategy.Register<ZoneHolding>("*", new ShapeAccessor());
        options.MemberAccessStrategy.Register<ShapeMetadata>();
        options.MemberAccessStrategy.Register<CultureInfo>();

        options.Scope.SetValue("Culture", new CultureValue());

        options.Scope.SetValue("Environment", new HostingEnvironmentValue(_hostEnvironment));

        options.Scope.SetValue("Request", new HttpRequestValue());

        options.Scope.SetValue("HttpContext", new HttpContextValue());

        options.Scope.SetValue("TrackingConsent", new TrackingConsentValue());

        options.MemberAccessStrategy.Register<FormCollection, FluidValue>((forms, name) =>
        {
            if (name == "Keys")
            {
                return new ArrayValue(forms.Keys.Select(x => new StringValue(x)).ToArray());
            }

            return new StringValuesValue(forms[name]);
        });

        options.MemberAccessStrategy.Register<HttpContextItemsWrapper, object>((httpContext, name) => httpContext.Items[name]);
        options.MemberAccessStrategy.Register<QueryCollection, StringValues>((queries, name) => queries[name]);
        options.MemberAccessStrategy.Register<CookieCollectionWrapper, string>((cookies, name) => cookies.RequestCookieCollection[name]);
        options.MemberAccessStrategy.Register<HeaderDictionaryWrapper, StringValues>((headers, name) => headers.HeaderDictionary[name]);
        options.MemberAccessStrategy.Register<RouteValueDictionaryWrapper, object>((headers, name) => headers.RouteValueDictionary[name]);
    }
}
