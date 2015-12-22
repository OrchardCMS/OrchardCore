using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Cache.Abstractions;
using System.Collections.Generic;

namespace Orchard.Environment.Cache.CacheContextProviders
{
    public class ThemeCacheContextProvider : ICacheContextProvider
    {
        private readonly IThemeManager _themeManager;

        public ThemeCacheContextProvider(IThemeManager themeManager)
        {
            _themeManager = themeManager;
        }

        public void PopulateContextEntries(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            // Always vary by theme
            entries.Add(new CacheContextEntry("theme", _themeManager.GetThemeAsync().Result.Name));
        }
    }
}
