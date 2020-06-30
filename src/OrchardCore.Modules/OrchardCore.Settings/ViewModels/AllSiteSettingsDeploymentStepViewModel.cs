using System.Collections.Generic;

namespace OrchardCore.Settings.ViewModels
{
    public class AllSiteSettingsDeploymentStepViewModel
    {
        public List<SiteSetting> AvailableSettings { get; set; }
        public string[] Settings { get; set; }
    }
}
