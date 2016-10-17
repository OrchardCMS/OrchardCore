using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Admin
{
    public interface IAdminThemeService
    {
        Task<ExtensionDescriptor> GetAdminThemeAsync();
        Task SetAdminThemeAsync(string themeName);
        Task<string> GetAdminThemeNameAsync();
    }
}
