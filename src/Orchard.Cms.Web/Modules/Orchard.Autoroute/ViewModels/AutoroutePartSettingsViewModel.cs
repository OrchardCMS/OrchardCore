using Orchard.Autoroute.Models;

namespace Orchard.Autoroute.ViewModels
{
    public class AutoroutePartSettingsViewModel
    {
        public bool AllowCustomPath { get; set; }
        public string Pattern { get; set; }
        public AutoroutePartSettings AutoroutePartSettings { get; set; }
    }
}
