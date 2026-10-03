using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Navigation;

internal static class NavigationAlternatesFactory
{
    private static readonly ConcurrentDictionary<string, string[]> s_navigationCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<NavigationItemAlternatesCacheKey, string[]> s_navigationItemCache = new();
    private static readonly ConcurrentDictionary<NavigationItemAlternatesCacheKey, string[]> s_navigationItemLinkCache = new();

    public static string[] GetNavigationAlternates(string menuName)
    {
        menuName ??= string.Empty;
        return s_navigationCache.GetOrAdd(menuName, static m => ["Navigation__" + m.EncodeAlternateElement()]);
    }

    public static string[] GetNavigationItemAlternates(string menuName, int level)
    {
        var key = new NavigationItemAlternatesCacheKey(menuName ?? string.Empty, level);

        return s_navigationItemCache.GetOrAdd(key, static k =>
        {
            var encodedMenuName = k.MenuName.EncodeAlternateElement();

            return
            [
                "NavigationItem__level__" + k.Level,
                "NavigationItem__" + encodedMenuName,
                "NavigationItem__" + encodedMenuName + "__level__" + k.Level
            ];
        });
    }

    public static string[] GetNavigationItemLinkAlternates(string menuName, int level)
    {
        var key = new NavigationItemAlternatesCacheKey(menuName ?? string.Empty, level);

        return s_navigationItemLinkCache.GetOrAdd(key, static k =>
        {
            var encodedMenuName = k.MenuName.EncodeAlternateElement();

            return
            [
                "NavigationItemLink__level__" + k.Level,
                "NavigationItemLink__" + encodedMenuName,
                "NavigationItemLink__" + encodedMenuName + "__level__" + k.Level
            ];
        });
    }

    private readonly record struct NavigationItemAlternatesCacheKey(
        string MenuName,
        int Level);
}
