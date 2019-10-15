using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.ContentLocalization.Settings
{
    public class LocalizationPartSettingsViewModel
    {
        public string Pattern { get; set; }

        [BindNever]
        public LocalizationPartSettings LocalizationPartSettings { get; set; }
    }
}
