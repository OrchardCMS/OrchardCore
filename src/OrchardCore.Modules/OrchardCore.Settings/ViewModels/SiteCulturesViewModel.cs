using System.Collections.Generic;

namespace OrchardCore.Settings.ViewModels
{
    public class SiteCulturesViewModel
    {
        public string CurrentCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IEnumerable<string> AvailableSystemCultures { get; set; }
    }
}