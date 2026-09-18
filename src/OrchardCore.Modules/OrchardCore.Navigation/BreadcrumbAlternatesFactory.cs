using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Navigation;

internal static class BreadcrumbAlternatesFactory
{
    private static readonly ConcurrentDictionary<BreadcrumbAlternatesCacheKey, string[]> s_breadcrumbCache = new();
    private static readonly ConcurrentDictionary<BreadcrumbItemAlternatesCacheKey, string[]> s_breadcrumbItemCache = new();

    public static string[] GetBreadcrumbAlternates(string name, string displayType)
    {
        var key = new BreadcrumbAlternatesCacheKey(name ?? string.Empty, displayType ?? string.Empty);

        return s_breadcrumbCache.GetOrAdd(key, static k =>
        {
            var encodedName = k.Name.EncodeAlternateElement();

            // The most specific alternate is the last one, which is the one the display engine looks for first.
            if (string.IsNullOrEmpty(k.DisplayType))
            {
                return ["Breadcrumb__" + encodedName];
            }

            var encodedDisplayType = k.DisplayType.EncodeAlternateElement();

            return
            [
                "Breadcrumb__" + encodedName,
                "Breadcrumb_" + encodedDisplayType,
                "Breadcrumb_" + encodedDisplayType + "__" + encodedName,
            ];
        });
    }

    public static string[] GetBreadcrumbItemAlternates(string name, string id, string displayType)
    {
        var key = new BreadcrumbItemAlternatesCacheKey(name ?? string.Empty, id ?? string.Empty, displayType ?? string.Empty);

        return s_breadcrumbItemCache.GetOrAdd(key, static k =>
        {
            var encodedName = k.Name.EncodeAlternateElement();
            var encodedId = k.Id.EncodeAlternateElement();
            var encodedDisplayType = k.DisplayType.EncodeAlternateElement();

            var alternates = new List<string>(7)
            {
                "BreadcrumbItem__" + encodedName,
            };

            if (encodedId.Length > 0)
            {
                alternates.Add("BreadcrumbItem__" + encodedId);
                alternates.Add("BreadcrumbItem__" + encodedName + "__" + encodedId);
            }

            if (encodedDisplayType.Length > 0)
            {
                alternates.Add("BreadcrumbItem_" + encodedDisplayType);
                alternates.Add("BreadcrumbItem_" + encodedDisplayType + "__" + encodedName);

                if (encodedId.Length > 0)
                {
                    alternates.Add("BreadcrumbItem_" + encodedDisplayType + "__" + encodedId);
                    alternates.Add("BreadcrumbItem_" + encodedDisplayType + "__" + encodedName + "__" + encodedId);
                }
            }

            return [.. alternates];
        });
    }

    private readonly record struct BreadcrumbAlternatesCacheKey(
        string Name,
        string DisplayType);

    private readonly record struct BreadcrumbItemAlternatesCacheKey(
        string Name,
        string Id,
        string DisplayType);
}
