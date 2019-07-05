namespace OrchardCore.Settings.ViewModels
{
    public class SiteSettingsViewModel
    {
        public string SiteName { get; set; }
        public string BaseUrl { get; set; }
        public string TimeZone { get; set; }
        public bool UseCdn { get; set; }
        public string CdnBaseUrl { get; set; }
        public ResourceDebugMode ResourceDebugMode { get; set; }
    }
}
