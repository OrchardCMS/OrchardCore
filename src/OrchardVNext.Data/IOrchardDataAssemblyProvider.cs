using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Data {
    public interface IOrchardDataAssemblyProvider : IDependency {
        IEnumerable<Assembly> CandidateAssemblies { get; }
    }
}