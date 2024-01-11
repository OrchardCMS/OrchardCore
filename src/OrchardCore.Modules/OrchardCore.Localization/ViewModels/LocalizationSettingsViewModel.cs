using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Localization.Models;

namespace OrchardCore.Localization.ViewModels
{
    /// <summary>
    /// Represents a view model for the localization settings.
    /// </summary>
    public class LocalizationSettingsViewModel
    {
        /// <summary>
        ///
        /// </summary>
        [BindNever]
        public CultureEntry[] Cultures { get; set; } = Array.Empty<CultureEntry>();

        /// <summary>
        /// Gets or sets all the supported cultures of the site. It also contains the default culture.
        /// </summary>
        /// <remarks>This property is a json array that is set in the editor</remarks>
        public string SupportedCultures { get; set; }

        /// <summary>
        /// Gets or sets the default culture of the site.
        /// </summary>
        public string DefaultCulture { get; set; }
    }
}
