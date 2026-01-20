using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.Environment.Cache.CacheContextProviders
{
    public class ThemeCacheContextProvider : ICacheContextProvider
    {
        private readonly IThemeManager _themeManager;

        public ThemeCacheContextProvider(IThemeManager themeManager)
        {
            _themeManager = themeManager;
        }

        public async Task PopulateContextEntriesAsync(IEnumerable<string> contexts, List<CacheContextEntry> entries)
        {
            // Always vary by theme

            var theme = await _themeManager.GetThemeAsync();
            entries.Add(new CacheContextEntry("theme", theme.Id));
        }
    }
}
