using System.Reflection;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting.Extensions.Loaders;
using System.IO;

namespace OrchardVNext.Hosting.Extensions {
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
            var projectPath = Path.Combine(_path, assemblyName.Name);
            if (!Project.HasProjectFile(projectPath)) {
                return null;
            }

            return _cache.Get<Assembly>(assemblyName.Name, cacheContext => {
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
                
                return engine.LoadProject(moduleContext.Project, null, _assemblyLoadContextAccessor.Default);
            });
        }
    }
}