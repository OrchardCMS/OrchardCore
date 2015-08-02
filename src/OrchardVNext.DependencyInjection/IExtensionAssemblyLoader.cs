using Microsoft.Dnx.Runtime;

namespace OrchardVNext.DependencyInjection {
    public interface IExtensionAssemblyLoader : IAssemblyLoader {
        IExtensionAssemblyLoader WithPath(string path);
    }
}