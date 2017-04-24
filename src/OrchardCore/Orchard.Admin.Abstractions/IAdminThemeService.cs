using System.Threading.Tasks;
using Orchard.Environment.Extensions;

namespace Orchard.Admin
{
    public interface IAdminThemeService
    {
        Task<IExtensionInfo> GetAdminThemeAsync();
        Task SetAdminThemeAsync(string themeName);
        Task<string> GetAdminThemeNameAsync();
    }
}
