using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;

namespace Orchard.DisplayManagement.Theming
{
    public interface IThemeManager
    {
        Task<ExtensionDescriptor> GetThemeAsync();
    }
}
