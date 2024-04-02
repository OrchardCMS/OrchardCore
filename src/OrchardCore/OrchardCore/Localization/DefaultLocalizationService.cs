using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a default implementation for <see cref="ILocalizationService"/>.
    /// </summary>
    public class DefaultLocalizationService : ILocalizationService
    {
        private static readonly Task<string> _defaultCulture = Task.FromResult(CultureInfo.InstalledUICulture.Name);
        private static readonly Task<string[]> _supportedCultures = Task.FromResult(new[] { CultureInfo.InstalledUICulture.Name });

        /// <inheritdocs />
        public Task<string> GetDefaultCultureAsync() => _defaultCulture;

        /// <inheritdocs />
        public Task<string[]> GetSupportedCulturesAsync() => _supportedCultures;
    }
}
