using OrchardCore.Autoroute.Models;

namespace OrchardCore.Autoroute.ViewModels
{
    public class AutoroutePartSettingsViewModel
    {
        public bool AllowCustomPath { get; set; }
        public string Pattern { get; set; }
        public bool ShowHomepageOption {get; set; }
        public AutoroutePartSettings AutoroutePartSettings { get; set; }
    }
}
