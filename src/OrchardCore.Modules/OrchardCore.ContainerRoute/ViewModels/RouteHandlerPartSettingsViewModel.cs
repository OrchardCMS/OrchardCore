using OrchardCore.ContainerRoute.Models;

namespace OrchardCore.ContainerRoute.ViewModels
{
    public class RouteHandlerPartSettingsViewModel
    {
        public bool AllowCustomPath { get; set; }
        public string Pattern { get; set; }
        public bool AllowUpdatePath { get; set; }
        public RouteHandlerPartSettings RouteHandlerPartSettings { get; set; }
    }
}
