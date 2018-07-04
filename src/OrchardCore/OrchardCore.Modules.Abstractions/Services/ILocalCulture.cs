using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Modules.Services
{
    public interface ILocalCulture
    {
        /// <summary>
        /// Returns the local culture.
        /// </summary>
        Task<CultureInfo> GetLocalCultureAsync();
    }
}
