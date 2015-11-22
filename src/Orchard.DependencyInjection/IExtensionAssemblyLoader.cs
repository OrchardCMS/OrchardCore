using Microsoft.Extensions.PlatformAbstractions;

namespace Orchard.DependencyInjection
{
    public interface IExtensionAssemblyLoader : IAssemblyLoader
    {
        IExtensionAssemblyLoader WithPath(string path);
    }
}