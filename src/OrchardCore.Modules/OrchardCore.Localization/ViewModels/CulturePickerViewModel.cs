using System.Collections.Generic;
using System.Globalization;

namespace OrchardCore.Localization.ViewModels
{
    public class CulturePickerViewModel
    {
        public string SelectedCulture { get; set; }

        public IList<CultureInfo> SupportedUICultures { get; set; }
    }
}
