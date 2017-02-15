using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Modules
{
    public interface IModularAssemblyProvider
    {
        IEnumerable<Assembly> GetAssemblies(ISet<string> referenceAssemblies);
    }
}
