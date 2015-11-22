using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.Extensions.PlatformAbstractions;
using Orchard.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Orchard.Hosting.Mvc
{
    public class OrchardMvcAssemblyProvider : IAssemblyProvider
    {
        /// <summary>
        /// Gets the set of assembly names that are used as root for discovery of
        /// MVC controllers, view components and views.
        /// </summary>
        // DefaultControllerTypeProvider uses CandidateAssemblies to determine if the base type of a POCO controller
        // lives in an assembly that references MVC. CandidateAssemblies excludes all assemblies from the
        // ReferenceAssemblies set. Consequently adding WebApiCompatShim to this set would cause the ApiController to
        // fail this test.
        protected virtual HashSet<string> ReferenceAssemblies { get; }
        = new HashSet<string>(StringComparer.Ordinal)
        {
            "Microsoft.AspNet.Mvc",
            "Microsoft.AspNet.Mvc.Abstractions",
            "Microsoft.AspNet.Mvc.ApiExplorer",
            "Microsoft.AspNet.Mvc.Core",
            "Microsoft.AspNet.Mvc.Cors",
            "Microsoft.AspNet.Mvc.DataAnnotations",
            "Microsoft.AspNet.Mvc.Formatters.Json",
            "Microsoft.AspNet.Mvc.Formatters.Xml",
            "Microsoft.AspNet.Mvc.Razor",
            "Microsoft.AspNet.Mvc.Razor.Host",
            "Microsoft.AspNet.Mvc.TagHelpers",
            "Microsoft.AspNet.Mvc.ViewFeatures",
            "Microsoft.AspNet.PageExecutionInstrumentation.Interfaces",
        };

        private readonly IOrchardLibraryManager _libraryManager;
        private readonly IAssemblyLoaderContainer _loaderContainer;
        private readonly IExtensionAssemblyLoader _extensionAssemblyLoader;

        public OrchardMvcAssemblyProvider(IOrchardLibraryManager libraryManager,
            IAssemblyLoaderContainer assemblyLoaderContainer,
            IExtensionAssemblyLoader extensionAssemblyLoader)
        {
            _libraryManager = libraryManager;
            _loaderContainer = assemblyLoaderContainer;
            _extensionAssemblyLoader = extensionAssemblyLoader;
        }

        /// <inheritdoc />
        public IEnumerable<Assembly> CandidateAssemblies
        {
            get
            {
                return GetCandidateLibraries().SelectMany(l => l.Assemblies)
                                              .Select(Load);
            }
        }

        /// <summary>
        /// Returns a list of libraries that references the assemblies in <see cref="ReferenceAssemblies"/>.
        /// By default it returns all assemblies that reference any of the primary MVC assemblies
        /// while ignoring MVC assemblies.
        /// </summary>
        /// <returns>A set of <see cref="ILibraryInformation"/>.</returns>
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