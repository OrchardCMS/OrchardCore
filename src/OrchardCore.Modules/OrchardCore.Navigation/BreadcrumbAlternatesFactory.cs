using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Navigation;

internal static class BreadcrumbAlternatesFactory
{
    private static readonly ConcurrentDictionary<string, string[]> s_breadcrumbCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<BreadcrumbItemAlternatesCacheKey, string[]> s_breadcrumbItemCache = new();

    public static string[] GetBreadcrumbAlternates(string name)
    {
        name ??= string.Empty;

        return s_breadcrumbCache.GetOrAdd(name, static n => ["Breadcrumb__" + n.EncodeAlternateElement()]);
    }

    public static string[] GetBreadcrumbItemAlternates(string name, string id)
    {
        var key = new BreadcrumbItemAlternatesCacheKey(name ?? string.Empty, id ?? string.Empty);

        return s_breadcrumbItemCache.GetOrAdd(key, static k =>
        {
            var encodedName = k.Name.EncodeAlternateElement();

            if (string.IsNullOrEmpty(k.Id))
            {
                return ["BreadcrumbItem__" + encodedName];
            }

            var encodedId = k.Id.EncodeAlternateElement();

            return
            [
                "BreadcrumbItem__" + encodedName,
                "BreadcrumbItem__" + encodedId,
                "BreadcrumbItem__" + encodedName + "__" + encodedId,
            ];
        });
    }

    private readonly record struct BreadcrumbItemAlternatesCacheKey(
        string Name,
        string Id);
}
