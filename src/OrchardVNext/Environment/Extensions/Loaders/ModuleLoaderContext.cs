using System;
using Microsoft.Framework.Runtime;

namespace OrchardVNext.Environment.Extensions.Loaders
{
    public class ModuleLoaderContext {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICache _cache;


        public ModuleLoaderContext(IServiceProvider serviceProvider,
                                       string projectDirectory,
                                       ICache cache) {
            _serviceProvider = serviceProvider;
            _cache = cache;

            ProjectDirectory = projectDirectory;
            RootDirectory = Microsoft.Framework.Runtime.ProjectResolver.ResolveRootDirectory(ProjectDirectory);

            // A new project resolver is required. you cannot reuse the one from the 
            // parent service provider as that will be for the parent context.
            ProjectResolver = new ProjectResolver(ProjectDirectory, RootDirectory);

            var referenceAssemblyDependencyResolver = new ReferenceAssemblyDependencyResolver(new FrameworkReferenceResolver());

            // Need to pass through package directory incase you download a package from the gallary, this needs to know about it
            var NuGetDependencyProvider = new NuGetDependencyResolver(
                NuGetDependencyResolver.ResolveRepositoryPath(RootDirectory), RootDirectory);
            var gacDependencyResolver = new GacDependencyResolver();
            var ProjectDepencyProvider = new ProjectReferenceDependencyProvider(ProjectResolver);
            var unresolvedDependencyProvider = new UnresolvedDependencyProvider();
            
            DependencyWalker = new DependencyWalker(new IDependencyProvider[] {
                ProjectDepencyProvider,
                NuGetDependencyProvider,
                referenceAssemblyDependencyResolver,
                gacDependencyResolver,
                unresolvedDependencyProvider
            });

            LibraryExportProvider = new CompositeLibraryExportProvider(new ILibraryExportProvider[] {
                new ModuleProjectLibraryExportProvider(
                    ProjectResolver, _serviceProvider),
                referenceAssemblyDependencyResolver,
                gacDependencyResolver,
                NuGetDependencyProvider
            });
        }
        public string RootDirectory { get; private set; }

        public string ProjectDirectory { get; private set; }

        public DependencyWalker DependencyWalker { get; private set; }
        public ILibraryExportProvider LibraryExportProvider { get; private set; }
        public IProjectResolver ProjectResolver { get; private set; }
    }
}