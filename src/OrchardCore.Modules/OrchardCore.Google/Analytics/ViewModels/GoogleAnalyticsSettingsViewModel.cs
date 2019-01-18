using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Google.Analytics.ViewModels
{
    public class GoogleAnalyticsSettingsViewModel
    {
        [Required(AllowEmptyStrings = false)]
        public string TrackingID { get; set; }
    }
}
