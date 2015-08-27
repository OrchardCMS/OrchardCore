using System.Reflection;
using System.Linq;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Orchard.DependencyInjection;
using Orchard.Hosting.Extensions.Loaders;
using System.IO;
using Microsoft.Dnx.Runtime.Loader;
using System.Collections.Generic;

namespace Orchard.Hosting.Extensions {
    public class ExtensionAssemblyLoader : IExtensionAssemblyLoader {
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ICache _cache;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IOrchardLibraryManager _libraryManager;
        private string _path;

        public ExtensionAssemblyLoader(
            IApplicationEnvironment applicationEnvironment,
            ICache cache,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IOrchardLibraryManager libraryManager) {
            _applicationEnvironment = applicationEnvironment;
            _cache = cache;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _libraryManager = libraryManager;
        }

        public IExtensionAssemblyLoader WithPath(string path) {
            _path = path;
            return this;
        }

        public Assembly Load(AssemblyName assemblyName) {
            return _cache.Get<Assembly>(assemblyName.Name, cacheContext => {
                var reference = _libraryManager.GetMetadataReference(assemblyName.Name);

                if (reference != null && reference is MetadataFileReference) {
                    var fileReference = (MetadataFileReference)reference;

                    var assembly = _assemblyLoadContextAccessor
                        .Default
                        .LoadFile(fileReference.Path);

                    return assembly;
                }

                var projectPath = Path.Combine(_path, assemblyName.Name);
                if (!Project.HasProjectFile(projectPath)) {
                    return null;
                }

                var moduleContext = new ModuleLoaderContext(
                    projectPath,
                    _applicationEnvironment.RuntimeFramework);

                foreach (var lib in moduleContext.LibraryManager.GetLibraries()) {
                    _libraryManager.AddLibrary(lib);
                }

                var engine = new CompilationEngine(new CompilationEngineContext(
                    _applicationEnvironment,
                    _assemblyLoadContextAccessor.Default,
                    new CompilationCache()));

                var exporter = engine.CreateProjectExporter(
                    moduleContext.Project, moduleContext.TargetFramework, _applicationEnvironment.Configuration);

                var exports = exporter.GetAllExports(moduleContext.Project.Name);
                foreach (var metadataReference in exports.MetadataReferences) {
                    _libraryManager.AddMetadataReference(metadataReference);

                }
                
                var loadedProjectAssembly = engine.LoadProject(
                    moduleContext.Project, null, _assemblyLoadContextAccessor.Default);

                IList<LibraryDependency> flattenedList = moduleContext
                    .Project
                    .Dependencies
                    .SelectMany(x => Flatten(x))
                    .Where(x => x.Library.Type == "Package")
                    .Distinct()
                    .ToList();

                foreach (var dependency in flattenedList) {
                    foreach (var assemblyToLoad in dependency.Library.Assemblies){
                        Assembly.Load(new AssemblyName(assemblyToLoad));
                    }
                }

                return loadedProjectAssembly;
            });
        }


        public static IList<LibraryDependency> Flatten(LibraryDependency root) {

            var flattened = new List<LibraryDependency> { root };

            var children = root.Library.Dependencies;

            if (children != null) {
                foreach (var child in children) {
                    flattened.AddRange(Flatten(child));
                }
            }

            return flattened;
        }
    }
}