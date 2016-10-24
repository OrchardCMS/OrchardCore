using Orchard.Environment.Extensions;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Theming
{
    public interface IThemeManager
    {
        Task<IExtensionInfo> GetThemeAsync();
    }
}
