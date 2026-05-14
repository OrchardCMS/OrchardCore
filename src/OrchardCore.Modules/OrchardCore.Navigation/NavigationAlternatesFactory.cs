using System.Collections.Concurrent;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Navigation;

internal static class NavigationAlternatesFactory
{
    private static readonly ConcurrentDictionary<string, string[]> _navigationCache = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<NavigationItemAlternatesCacheKey, string[]> _navigationItemCache = new();
    private static readonly ConcurrentDictionary<NavigationItemAlternatesCacheKey, string[]> _navigationItemLinkCache = new();

    public static string[] GetNavigationAlternates(string menuName)
    {
        menuName ??= string.Empty;
        return _navigationCache.GetOrAdd(menuName, static m => ["Navigation__" + m.EncodeAlternateElement()]);
    }

    public static string[] GetNavigationItemAlternates(string menuName, int level)
    {
        var key = new NavigationItemAlternatesCacheKey(menuName ?? string.Empty, level);

        return _navigationItemCache.GetOrAdd(key, static k =>
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

        return _navigationItemLinkCache.GetOrAdd(key, static k =>
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
