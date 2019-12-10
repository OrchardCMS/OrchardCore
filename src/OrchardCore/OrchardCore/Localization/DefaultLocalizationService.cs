using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a default implementation for <see cref="ILocalizationService"/>.
    /// </summary>
    public class DefaultLocalizationService : ILocalizationService
    {
        private static readonly Task<string> DefaultCulture = Task.FromResult(CultureInfo.InstalledUICulture.Name);
        private static readonly Task<string[]> SupportedCultures = Task.FromResult(new[] { CultureInfo.InstalledUICulture.Name });

        /// <inheritdocs />
        public Task<string> GetDefaultCultureAsync() => DefaultCulture;

        /// <inheritdocs />
        public Task<string[]> GetSupportedCulturesAsync() => SupportedCultures;
    }
}
