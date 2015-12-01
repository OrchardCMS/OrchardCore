using Orchard.Environment.Extensions.Models;
using Orchard.DependencyInjection;

namespace Orchard.DisplayManagement.Theming
{
    public interface IThemeManager : IDependency
    {
        ExtensionDescriptor GetTheme();
    }
}
