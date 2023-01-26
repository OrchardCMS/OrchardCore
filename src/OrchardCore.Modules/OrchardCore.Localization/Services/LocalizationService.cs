using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Entities;
using OrchardCore.Localization.Models;
using OrchardCore.Settings;

namespace OrchardCore.Localization.Services
{
    /// <summary>
    /// Represents a localization service.
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private static readonly string DefaultCulture = CultureInfo.InstalledUICulture.Name;
        private static readonly string[] SupportedCultures = new[] { CultureInfo.InstalledUICulture.Name };

        private readonly ISiteService _siteService;
        private readonly IEnumerable<ICultureAliasProvider> _cultureAliasProviders;

        private LocalizationSettings _localizationSettings;

        /// <summary>
        /// Creates a new instance of <see cref="LocalizationService"/>.
        /// </summary>
        /// <param name="siteService">The <see cref="ISiteService"/>.</param>
        /// <param name="cultureAliasProviders">The <see cref="IEnumerable{ICultureAliasProvider}"/>.</param>
        public LocalizationService(ISiteService siteService, IEnumerable<ICultureAliasProvider> cultureAliasProviders)
        {
            _siteService = siteService;
            _cultureAliasProviders = cultureAliasProviders;
        }

        /// <inheritdocs />
        public async Task<string> GetDefaultCultureAsync()
        {
            await InitializeLocalizationSettingsAsync();

            return _localizationSettings.DefaultCulture ?? DefaultCulture;
        }

        /// <inheritdocs />
        public async Task<string[]> GetSupportedCulturesAsync()
        {
            await InitializeLocalizationSettingsAsync();

            var supportedCultures = _localizationSettings.SupportedCultures == null || _localizationSettings.SupportedCultures.Length == 0
                ? SupportedCultures
                : _localizationSettings.SupportedCultures;

            var mappedSupportedCultures = new List<string>();

            foreach (var supportedCulture in supportedCultures)
            {
                foreach (var provider in _cultureAliasProviders)
                {
                    if (provider.TryGetCulture(supportedCulture, out var culture))
                    {
                        if (!supportedCultures.Contains(culture.Name))
                        {
                            mappedSupportedCultures.Add(culture.Name);
                        }

                        break;
                    }
                }
            }

            return supportedCultures
                .Union(mappedSupportedCultures)
                .ToArray();
        }

        private async Task InitializeLocalizationSettingsAsync()
        {
            if (_localizationSettings == null)
            {
                var siteSettings = await _siteService.GetSiteSettingsAsync();
                _localizationSettings = siteSettings.As<LocalizationSettings>();
            }
        }
    }
}
