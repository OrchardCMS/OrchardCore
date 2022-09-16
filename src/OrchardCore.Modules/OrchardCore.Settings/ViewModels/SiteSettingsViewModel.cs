using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Settings.ViewModels
{
    public class SiteSettingsViewModel
    {
        public string SiteName { get; set; }
        public string PageTitleFormat { get; set; }
        public string BaseUrl { get; set; }
        public string TimeZone { get; set; }

        [Required]
        public int? PageSize { get; set; }

        public bool UseCdn { get; set; }
        public string CdnBaseUrl { get; set; }
        public ResourceDebugMode ResourceDebugMode { get; set; }
        public bool AppendVersion { get; set; }
        public string Meta { get; set; }
        public CacheMode CacheMode { get; set; }
    }
}
