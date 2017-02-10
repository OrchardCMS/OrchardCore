using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Modules
{
    public interface IModularAssemblyProvider
    {
        IEnumerable<Assembly> GetAssemblies();
        IEnumerable<Assembly> GetAssemblies(IEnumerable<Assembly> runtimeAssemblies, ISet<string> referenceAssemblies);
    }
}
