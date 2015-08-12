using System;
using System.Runtime.Versioning;
using Microsoft.Dnx.Compilation;
using Microsoft.Dnx.Compilation.Caching;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using NuGet;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class ModuleLoaderContext {
        public ModuleLoaderContext(
            IServiceProvider hostServices,
            Project project, 
            string configuration, 
            FrameworkName targetFramework) {
            var cacheContextAccessor = new CacheContextAccessor();
            var cache = new Cache(cacheContextAccessor);

            var applicationHostContext = new ApplicationHostContext(
                hostServices: hostServices,
                projectDirectory: project.ProjectDirectory,
                packagesDirectory: null,
                configuration: configuration,
                targetFramework: targetFramework,
                cache: cache,
                cacheContextAccessor: cacheContextAccessor,
                namedCacheDependencyProvider: NamedCacheDependencyProvider.Empty);
            
            DependencyWalker = applicationHostContext.DependencyWalker;
            FrameworkName = targetFramework;
            CacheContextAccessor = cacheContextAccessor;
            Cache = cache;
            LibraryExportProvider = applicationHostContext.LibraryExportProvider;
            ServiceProvider =  applicationHostContext.ServiceProvider;
            AssemblyLoadContextFactory = applicationHostContext.AssemblyLoadContextFactory;
            LibraryManager = applicationHostContext.LibraryManager;

            Walk(project.Name, project.Version);
        }

        public DependencyWalker DependencyWalker { get; set; }
        public FrameworkName FrameworkName { get; set; }
        public ICacheContextAccessor CacheContextAccessor { get; set; }
        public ICache Cache { get; set; }
        public ILibraryExportProvider LibraryExportProvider { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public IAssemblyLoadContextFactory AssemblyLoadContextFactory { get; set; }
        public ILibraryManager LibraryManager { get; set; }

        private void Walk(string projectName, SemanticVersion projectVersion) {
            DependencyWalker.Walk(projectName, projectVersion, FrameworkName);
        }
    }
}