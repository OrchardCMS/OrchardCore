using System.Threading.Tasks;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Theming
{
    public interface IThemeManager
    {
        Task<IExtensionInfo> GetThemeAsync();
    }
}
