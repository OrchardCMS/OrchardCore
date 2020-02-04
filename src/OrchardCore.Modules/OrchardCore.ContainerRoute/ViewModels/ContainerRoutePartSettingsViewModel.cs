using OrchardCore.ContainerRoute.Models;

namespace OrchardCore.ContainerRoute.ViewModels
{
    public class ContainerRoutePartSettingsViewModel
    {
        public bool AllowCustomPath { get; set; }
        public string Pattern { get; set; }
        public bool ShowHomepageOption {get; set; }
        public bool AllowUpdatePath { get; set; }
        public ContainerRoutePartSettings ContainerRoutePartSettings { get; set; }
    }
}
