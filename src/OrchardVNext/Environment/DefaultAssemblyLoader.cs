using System;
using System.Reflection;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.CSharp;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Caching;
using Microsoft.Dnx.Runtime.Infrastructure;
using Microsoft.Dnx.Runtime.Loader;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Environment.Extensions.Loaders;
using OrchardVNext.FileSystems.VirtualPath;

namespace OrchardVNext.Environment
{
    public class ExtensionAssemblyLoader : IAssemblyLoader {
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IFileWatcher _fileWatcher;
        private readonly IOrchardLibraryManager _orchardLibraryManager;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IVirtualPathProvider _virtualPathProvider;private readonly string _path;
        
        public ExtensionAssemblyLoader(string path, IServiceProvider serviceProvider) {
            _path = path;
            _serviceProvider = serviceProvider;
            _applicationEnvironment = serviceProvider.GetService<IApplicationEnvironment>();
            _fileWatcher = serviceProvider.GetService<IFileWatcher>();
            _orchardLibraryManager = serviceProvider.GetService<IOrchardLibraryManager>();
            _assemblyLoadContextAccessor = serviceProvider.GetService<IAssemblyLoadContextAccessor>();
            _virtualPathProvider = serviceProvider.GetService<IVirtualPathProvider>();
        }

        public Assembly Load(AssemblyName assemblyName) {
            Project project;
            string name = assemblyName.FullName;
            if (!Project.TryGetProject(_virtualPathProvider.Combine(_path, name), out project)) {
                return null;
            }

            var cache = (ICache)_serviceProvider.GetService(typeof(ICache));

            var target = new CompilationTarget(
                name,
                project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName,
                _applicationEnvironment.Configuration,
                null);

            ModuleLoaderContext moduleContext = new ModuleLoaderContext(
                _serviceProvider,
                project.ProjectDirectory);

            moduleContext.DependencyWalker.Walk(name, project.Version, target.TargetFramework);

            var cacheContextAccessor = (ICacheContextAccessor)_serviceProvider.GetService(typeof(ICacheContextAccessor));
            var loadContextFactory = (IAssemblyLoadContextFactory)_serviceProvider.GetService(typeof(IAssemblyLoadContextFactory)) ?? new AssemblyLoadContextFactory(_serviceProvider);

            var compiler = new InternalRoslynCompiler(
               cache,
               cacheContextAccessor,
               new NamedCacheDependencyProvider(),
               loadContextFactory,
               _fileWatcher,
               _applicationEnvironment,
               _serviceProvider);

            _orchardLibraryManager.AddAdditionalRegistrations(moduleContext.DependencyWalker.Libraries);

            var exports = ProjectExportProviderHelper.GetExportsRecursive(
                _orchardLibraryManager,
                moduleContext.LibraryExportProvider,
                new CompilationTarget(name, target.TargetFramework, target.Configuration, target.Aspect),
                true);

            _orchardLibraryManager.AddAdditionalLibraryExportRegistrations(name, exports);

            foreach (var dependency in project.Dependencies) {
                if (!_orchardLibraryManager.MetadataReferences.ContainsKey(dependency.Name))
                    continue;
                
                exports.MetadataReferences.Add(_orchardLibraryManager.MetadataReferences[dependency.Name]);
            }

            var compliationContext = compiler.CompileProject(
                project.ToCompilationContext(target),
                exports.MetadataReferences,
                exports.SourceReferences,
                () => CompositeResourceProvider.Default.GetResources(project));
            
            var roslynProjectReference = new RoslynProjectReference(compliationContext);

            _orchardLibraryManager.AddMetadataReference(name, roslynProjectReference);

            var loadContext = _assemblyLoadContextAccessor.Default;
            return roslynProjectReference.Load(loadContext);
        }

        
    }
}