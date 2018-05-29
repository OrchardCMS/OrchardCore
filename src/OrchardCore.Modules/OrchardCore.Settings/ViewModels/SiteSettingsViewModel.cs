using System.Collections.Generic;
using OrchardCore.Modules;

namespace OrchardCore.Settings.ViewModels
{
    public class SiteSettingsViewModel
    {
        public string SiteName { get; set; }
        public string BaseUrl { get; set; }
        public string TimeZone { get; set; }
        public ITimeZone[] TimeZones { get; set; }
        public string SiteCulture { get; set; }
    }
}
