using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;

namespace OrchardCore.Mvc.LocationExpander;

internal sealed class CompositeViewLocationExpanderProvider : IViewLocationExpanderProvider
{
    // The ordered providers of the current request are cached in the 'HttpContext' items.
    private static readonly object s_providersKey = new();

    public int Priority
    {
        get { throw new NotSupportedException(); }
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var expanderProviders = DiscoverProviders(context);

        foreach (var provider in expanderProviders)
        {
            viewLocations = provider.ExpandViewLocations(context, viewLocations);
        }

        return viewLocations;
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        var expanderProviders = DiscoverProviders(context);

        foreach (var provider in expanderProviders)
        {
            provider.PopulateValues(context);
        }
    }

    private static IViewLocationExpanderProvider[] DiscoverProviders(ViewLocationExpanderContext context)
    {
        var httpContext = context.ActionContext.HttpContext;

        // The providers are scoped services that are resolved and sorted once per request, instead of
        // for each of the many view lookups a request performs.
        if (httpContext.Items.TryGetValue(s_providersKey, out var cached))
        {
            return (IViewLocationExpanderProvider[])cached;
        }

        var providers = httpContext.RequestServices
            .GetServices<IViewLocationExpanderProvider>()
            .OrderBy(x => x.Priority)
            .ToArray();

        httpContext.Items[s_providersKey] = providers;

        return providers;
    }
}
