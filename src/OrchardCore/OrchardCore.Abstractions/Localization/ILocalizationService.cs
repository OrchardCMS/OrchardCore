using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a contract for a localization service.
    /// </summary>
    public interface ILocalizationService
    {
        private static readonly CultureInfo[] _cultureAliases = new[]
        {
            CultureInfo.GetCultureInfo("zh-CN"),
            CultureInfo.GetCultureInfo("zh-TW")
        };

        /// <summary>
        /// Returns the default culture of the site.
        /// </summary>
        Task<string> GetDefaultCultureAsync();

        /// <summary>
        /// Returns all the supported cultures of the site. It also contains the default culture.
        /// </summary>
        Task<string[]> GetSupportedCulturesAndAliasesAsync();

        /// <summary>
        /// Gets a list of supported cultures by operating system.
        /// </summary>
        static CultureInfo[] GetCultures()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Union(_cultureAliases)
                .OrderBy(c => c.Name);

            return cultures.ToArray();
        }
    }
}
