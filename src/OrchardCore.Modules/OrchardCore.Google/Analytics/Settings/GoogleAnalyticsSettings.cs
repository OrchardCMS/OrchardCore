using Microsoft.AspNetCore.Http;

namespace OrchardCore.Google.Analytics.Settings
{
    public class GoogleAnalyticsSettings
    {
        public string TrackingID { get; set; }
        public bool ScriptsAtHead { get; set; }
    }
}
