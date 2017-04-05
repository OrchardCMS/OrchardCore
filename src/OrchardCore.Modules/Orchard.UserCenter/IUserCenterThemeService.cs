using System.Threading.Tasks;
using Orchard.Environment.Extensions;

namespace Orchard.UserCenter
{
    public interface IUserCenterThemeService
    {
        Task<IExtensionInfo> GetUserCenterThemeAsync();
        Task SetUserCenterThemeAsync(string themeName);
        Task<string> GetUserCenterThemeNameAsync();
    }
}
