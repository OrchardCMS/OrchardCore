using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Compilation.FileSystem;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Compilation;
using Microsoft.Dnx.Runtime.Loader;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.FileSystem.VirtualPath;
using OrchardVNext.Environment.Extensions;

namespace OrchardVNext.Environment
{
    public class ExtensionAssemblyLoader : IExtensionAssemblyLoader {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ICache _cache;
        private readonly IPackageAssemblyLookup _packageAssemblyLookup;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IOrchardLibraryManager _libraryManager;
        private string _path;

        private ModuleLoaderContext _moduleContext;

        public ExtensionAssemblyLoader(IServiceProvider serviceProvider,
            IApplicationEnvironment applicationEnvironment,
            IVirtualPathProvider virtualPathProvider,
            ICache cache,
            IPackageAssemblyLookup packageAssemblyLookup,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IOrchardLibraryManager libraryManager) {
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _virtualPathProvider = virtualPathProvider;
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
            if (!Project.TryGetProject(_virtualPathProvider.Combine(_path, name), out project)) {
                return null;
            }

            return _cache.Get<Assembly>(assemblyName.Name, cacheContext => {
                var target = new CompilationTarget(
                    name,
                    project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName,
                    _applicationEnvironment.Configuration,
                    null);

                _moduleContext = new ModuleLoaderContext(
                    _serviceProvider,
                    project.ProjectDirectory,
                    target.Configuration,
                    target.TargetFramework);

                foreach (var lib in _moduleContext.DependencyWalker.Libraries) {
                    _libraryManager.AddLibrary(lib.ToLibrary());
                }

                var compilationEngineFactory = new CompilationEngineFactory(
                    NoopWatcher.Instance, new CompilationCache());

                var engine = compilationEngineFactory.CreateEngine(new CompilationEngineContext(
                    _moduleContext.LibraryManager,
                    _moduleContext.ProjectGraphProvider,
                    _moduleContext.ServiceProvider,
                    target.TargetFramework,
                    target.Configuration));
                
                _packageAssemblyLookup.AddLoader(
                    _moduleContext.NuGetDependencyProvider);
                
                var p = engine.LoadProject(project, null, _assemblyLoadContextAccessor.Default);

                var exports = engine.RootLibraryExporter.GetExport(project.Name);
                foreach (var metadataReference in exports.MetadataReferences) {
                    _libraryManager.AddMetadataReference(metadataReference);
                }
                
                return p;
            });
        }
    }


    public interface IPackageAssemblyLookup {
        void AddLoader(NuGetDependencyResolver nugetLoader);
        NuGetDependencyResolver GetLoaderForPackage(string name);
    }

    public class PackageAssemblyLookup : IPackageAssemblyLookup {
        private IList<NuGetDependencyResolver> Loaders { get; } = new List<NuGetDependencyResolver>();

        public void AddLoader(NuGetDependencyResolver nugetLoader) {
            Loaders.Add(nugetLoader);
        }

        public NuGetDependencyResolver GetLoaderForPackage(string name) {
            return Loaders.FirstOrDefault(loader => loader.PackageAssemblyLookup.Any(x => x.Key.Name == name));
        }
    }
}