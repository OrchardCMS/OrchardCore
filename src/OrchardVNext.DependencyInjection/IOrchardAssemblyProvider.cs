using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Runtime;

namespace OrchardVNext.DependencyInjection {
    public interface IOrchardAssemblyProvider : IDependency {
        IEnumerable<Assembly> CandidateAssemblies { get; }
    }

    public class OrchardAssemblyProvider : IOrchardAssemblyProvider {
        private readonly IOrchardLibraryManager _libraryManager;
        private readonly IAssemblyLoaderContainer _loaderContainer;
        private readonly IExtensionAssemblyLoader _extensionAssemblyLoader;

        public OrchardAssemblyProvider(IOrchardLibraryManager libraryManager,
            IAssemblyLoaderContainer assemblyLoaderContainer,
            IExtensionAssemblyLoader extensionAssemblyLoader) {
            _libraryManager = libraryManager;
            _loaderContainer = assemblyLoaderContainer;
            _extensionAssemblyLoader = extensionAssemblyLoader;
        }

        public IEnumerable<Assembly> CandidateAssemblies {
            get {
                return GetCandidateLibraries().SelectMany(l => l.Assemblies)
                                              .Select(Load);
            }
        }

        protected virtual IEnumerable<Library> GetCandidateLibraries() {
            return _libraryManager.GetLibraries()
                                      .Distinct();
        }

        private Assembly Load(AssemblyName assemblyName) {
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
                return assembly;

            using (_loaderContainer.AddLoader(_extensionAssemblyLoader)) {
                return Assembly.Load(assemblyName);
            }
        }
    }
}