using System;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Localization.ViewModels
{
    public class LocalizationSettingsViewModel
    {
        [BindNever]
        public CultureEntry[] Cultures { get; set; } = Array.Empty<CultureEntry>();

        public string SupportedCultures { get; set; }
        public string DefaultCulture { get; set; }
    }

    public class CultureEntry
    {
        public CultureInfo CultureInfo { get; set; }
        public bool IsDefault { get; set; }
        public bool Supported { get; set; }
    }
}
