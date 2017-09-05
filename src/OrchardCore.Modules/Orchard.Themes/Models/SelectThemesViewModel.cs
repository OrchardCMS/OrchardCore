using System.Collections.Generic;

namespace OrchardCore.Themes.Models
{
    public class SelectThemesViewModel
    {
        public ThemeEntry CurrentSiteTheme { get; set; }
        public ThemeEntry CurrentAdminTheme { get; set; }
        public IEnumerable<ThemeEntry> Themes { get; set; }
    }
}
