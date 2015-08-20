using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Compilation.FileSystem;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Compilation;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting.Extensions.Loaders;
using System.IO;

namespace OrchardVNext.Hosting.Extensions {
    public class ExtensionAssemblyLoader : IExtensionAssemblyLoader {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly ICache _cache;
        private readonly IPackageAssemblyLookup _packageAssemblyLookup;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IOrchardLibraryManager _libraryManager;
        private string _path;

        private ModuleLoaderContext _moduleContext;

        public ExtensionAssemblyLoader(IServiceProvider serviceProvider,
            IApplicationEnvironment applicationEnvironment,
            ICache cache,
            IPackageAssemblyLookup packageAssemblyLookup,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IOrchardLibraryManager libraryManager) {
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _cache = cache;
            _packageAssemblyLookup = packageAssemblyLookup;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _libraryManager = libraryManager;
        }

        public IExtensionAssemblyLoader WithPath(string path) {
            _path = path;
            return this;
        }

        public Assembly Load(AssemblyName assemblyName) {
            Project project;
            string name = assemblyName.FullName;
            if (!Project.TryGetProject(Path.Combine(_path, name), out project)) {
                return null;
            }

            return _cache.Get<Assembly>(assemblyName.Name, cacheContext => {
                var target = new CompilationTarget(
                    name,
                    project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName,
                    _applicationEnvironment.Configuration,
                    null);

                _moduleContext = new ModuleLoaderContext(
                    project.ProjectDirectory,
                    target.TargetFramework);
                
                foreach (var lib in _moduleContext.LibraryManager.GetLibraries()) {
                    _libraryManager.AddLibrary(lib);
                }

                var engine = new CompilationEngine(new CompilationEngineContext(
                    _applicationEnvironment,
                    _assemblyLoadContextAccessor.Default,
                    new CompilationCache()));
                
                _packageAssemblyLookup.AddPath(
                    _moduleContext.PackagesDirectory);
                
                var p = engine.LoadProject(project, null, _assemblyLoadContextAccessor.Default);

                var exporter = engine.CreateProjectExporter(project, target.TargetFramework, target.Configuration);

                var exports = exporter.GetExport(project.Name);
                foreach (var metadataReference in exports.MetadataReferences) {
                    _libraryManager.AddMetadataReference(metadataReference);
                }
                
                return p;
            });
        }
    }


    public interface IPackageAssemblyLookup {
        IList<string> PackagePaths { get; }
        void AddPath(string packagePath);
    }

    public class PackageAssemblyLookup : IPackageAssemblyLookup {
        public IList<string> PackagePaths { get; } = new List<string>();

        public void AddPath(string packagePath) {
            if (!PackagePaths.Contains(packagePath)) {
                PackagePaths.Add(packagePath);
            }
        }
    }
}