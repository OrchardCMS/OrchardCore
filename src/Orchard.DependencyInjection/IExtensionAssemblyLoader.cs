using Microsoft.Dnx.Runtime;

namespace Orchard.DependencyInjection {
    public interface IExtensionAssemblyLoader : IAssemblyLoader {
        IExtensionAssemblyLoader WithPath(string path);
    }
}