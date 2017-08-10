using System.Collections.Generic;

namespace Orchard.Themes.Models
{
    public class SelectThemesViewModel
    {
        public ThemeEntry CurrentSiteTheme { get; set; }
        public ThemeEntry CurrentAdminTheme { get; set; }
        public IEnumerable<ThemeEntry> Themes { get; set; }
    }
}
