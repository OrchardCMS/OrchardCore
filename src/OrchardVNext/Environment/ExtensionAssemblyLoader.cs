using System;
using System.Linq;
using System.Reflection;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Runtime;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.FileSystem.VirtualPath;

namespace OrchardVNext.Environment
{
    public class ExtensionAssemblyLoader : IExtensionAssemblyLoader {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IFileWatcher _fileWatcher;
        private readonly IOrchardLibraryManager _orchardLibraryManager;
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly ICache _cache;
        private string _path;
        
        public ExtensionAssemblyLoader(IServiceProvider serviceProvider,
            IApplicationEnvironment applicationEnvironment,
            IFileWatcher fileWatcher,
            IOrchardLibraryManager orchardLibraryManager,
            IVirtualPathProvider virtualPathProvider,
            ICache cache) {
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _fileWatcher = fileWatcher;
            _orchardLibraryManager = orchardLibraryManager;
            _virtualPathProvider = virtualPathProvider;
            _cache = cache;
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

            return _cache.Get<Assembly>(assemblyName.Name, (o) => {
                var target = new CompilationTarget(
                    name,
                    project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName,
                    _applicationEnvironment.Configuration,
                    null);

                var moduleContext = new ModuleLoaderContext(
                    _serviceProvider,
                    project,
                    target.Configuration,
                    target.TargetFramework);

                foreach (var library in moduleContext.DependencyWalker.Libraries) {
                    _orchardLibraryManager.AddLibrary(library.ToLibrary());
                }
                
                var exports = ProjectExportProviderHelper.GetExportsRecursive(
                    moduleContext.LibraryManager,
                    moduleContext.LibraryExportProvider,
                    target,
                    dependenciesOnly: true);
                
                _orchardLibraryManager.AddAdditionalLibraryExportRegistrations(name, exports);

                foreach (var dependency in project.Dependencies) {
                    if (!_orchardLibraryManager.MetadataReferences.ContainsKey(dependency.Name))
                        continue;

                    exports.MetadataReferences.Add(_orchardLibraryManager.MetadataReferences[dependency.Name]);
                }

                var roslynProjectCompiler = new RoslynProjectCompiler(
                    moduleContext.Cache,
                    moduleContext.CacheContextAccessor,
                    NamedCacheDependencyProvider.Empty,
                    moduleContext.AssemblyLoadContextFactory,
                    _fileWatcher,
                    _applicationEnvironment,
                    moduleContext.ServiceProvider);

                IMetadataProjectReference roslynProjectReference = roslynProjectCompiler.CompileProject(
                    project.ToCompilationContext(target),
                    () => new LibraryExport(exports.MetadataReferences, exports.SourceReferences),
                    () => CompositeResourceProvider.Default.GetResources(project)
                );


                //var libraryExport = moduleContext.LibraryExportProvider.GetLibraryExport(target);

                //IMetadataReference other = (IMetadataProjectReference)libraryExport.MetadataReferences.First(x => x.GetType() == typeof(IMetadataProjectReference));

                

                _orchardLibraryManager.AddMetadataReference(name, roslynProjectReference);

                var loadContext = moduleContext
                    .AssemblyLoadContextFactory
                    .Create(moduleContext.ServiceProvider);

                return roslynProjectReference.Load(loadContext);
            });
        }
    }
}