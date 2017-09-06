using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Admin
{
    public interface IAdminThemeService
    {
        Task<IExtensionInfo> GetAdminThemeAsync();
        Task SetAdminThemeAsync(string themeName);
        Task<string> GetAdminThemeNameAsync();
    }
}
