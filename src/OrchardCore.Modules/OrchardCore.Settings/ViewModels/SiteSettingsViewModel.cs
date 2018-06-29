using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Settings.ViewModels
{
    public class SiteSettingsViewModel
    {
        public string SiteName { get; set; }
        public string BaseUrl { get; set; }
        public string TimeZone { get; set; }
        public string Culture { get; set; }
        public IEnumerable<CultureInfo> SiteCultures { get; set; }
    }
}
