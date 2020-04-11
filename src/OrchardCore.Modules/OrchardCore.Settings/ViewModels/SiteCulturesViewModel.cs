using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Settings.ViewModels
{
    public class SiteCulturesViewModel
    {
        public string CurrentCulture { get; set; }
        public IEnumerable<string> SiteCultures { get; set; }
        public IEnumerable<CultureInfo> AvailableSystemCultures { get; set; }
    }
}
