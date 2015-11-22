using System.Collections.Generic;
using System.Reflection;
using Orchard.DependencyInjection;

namespace Orchard.Data
{
    public interface IOrchardDataAssemblyProvider : IDependency
    {
        IEnumerable<Assembly> CandidateAssemblies { get; }
    }
}