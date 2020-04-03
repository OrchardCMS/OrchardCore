using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    public class DefaultLocalizationService : ILocalizationService
    {
        private static readonly Task<string> DefaultCulture = Task.FromResult(CultureInfo.InstalledUICulture.Name);
        private static readonly Task<string[]> SupportedCultures = Task.FromResult(new[] { CultureInfo.InstalledUICulture.Name });

        public Task<string> GetDefaultCultureAsync()
        {
            return DefaultCulture;
        }

        public Task<string[]> GetSupportedCulturesAsync()
        {
            return SupportedCultures;
        }
    }
}
