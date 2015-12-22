using Orchard.Environment.Extensions.Models;
using Orchard.DependencyInjection;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Theming
{
    public interface IThemeManager : IDependency
    {
        Task<ExtensionDescriptor> GetThemeAsync();
    }
}
