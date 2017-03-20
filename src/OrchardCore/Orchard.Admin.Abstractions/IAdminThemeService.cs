using System.Threading.Tasks;
using OrchardCore.Extensions;

namespace Orchard.Admin
{
    public interface IAdminThemeService
    {
        Task<IExtensionInfo> GetAdminThemeAsync();
        Task SetAdminThemeAsync(string themeName);
        Task<string> GetAdminThemeNameAsync();
    }
}
