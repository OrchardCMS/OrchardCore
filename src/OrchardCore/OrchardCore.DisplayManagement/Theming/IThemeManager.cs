using OrchardCore.Environment.Extensions;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Theming
{
    public interface IThemeManager
    {
        Task<IExtensionInfo> GetThemeAsync();
    }
}
