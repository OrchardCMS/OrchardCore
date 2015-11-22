using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Orchard.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace Orchard.Data
{
    public class OrchardDataAssemblyProvider : IOrchardDataAssemblyProvider
    {
        private readonly IOrchardLibraryManager _libraryManager;
        private readonly IAssemblyLoaderContainer _loaderContainer;
        private readonly IExtensionAssemblyLoader _extensionAssemblyLoader;

        protected virtual HashSet<string> ReferenceAssemblies { get; }
            = new HashSet<string>(StringComparer.Ordinal)
            {
                        "Orchard.Data",
            };

        public OrchardDataAssemblyProvider(IOrchardLibraryManager libraryManager,
            IAssemblyLoaderContainer assemblyLoaderContainer,
            IExtensionAssemblyLoader extensionAssemblyLoader)
        {
            _libraryManager = libraryManager;
            _loaderContainer = assemblyLoaderContainer;
            _extensionAssemblyLoader = extensionAssemblyLoader;
        }

        public IEnumerable<Assembly> CandidateAssemblies
        {
            get
            {
                return GetCandidateLibraries().SelectMany(l => l.Assemblies)
                                              .Select(Load);
            }
        }

        protected virtual IEnumerable<Library> GetCandidateLibraries()
        {
            if (ReferenceAssemblies == null)
            {
                return Enumerable.Empty<Library>();
            }

            // GetReferencingLibraries returns the transitive closure of referencing assemblies
            // for a given assembly.
            return ReferenceAssemblies.SelectMany(_libraryManager.GetReferencingLibraries)
                                      .Distinct()
                                      .Where(IsCandidateLibrary);
        }

        private Assembly Load(AssemblyName assemblyName)
        {
            var assembly = Assembly.Load(assemblyName);
            if (assembly != null)
                return assembly;

            using (_loaderContainer.AddLoader(_extensionAssemblyLoader))
            {
                return Assembly.Load(assemblyName);
            }
        }

        private bool IsCandidateLibrary(Library library)
        {
            Debug.Assert(ReferenceAssemblies != null);
            return !ReferenceAssemblies.Contains(library.Name);
        }
    }
}