using System.Globalization;
using System.Threading.Tasks;

namespace OrchardCore.Localization.Services
{
    public interface ILocalCulture
    {
        /// <summary>
        /// Returns the local culture.
        /// </summary>
        Task<CultureInfo> GetLocalCultureAsync();
    }
}
