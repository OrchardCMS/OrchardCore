using System;
using System.IO;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.Compilation;
using Microsoft.Framework.PackageManager;
using NuGet;

namespace OrchardVNext.Environment.Extensions.Loaders
{
    public class ModuleLoaderContext {
        public ModuleLoaderContext(IServiceProvider serviceProvider,
                                       string projectDirectory) {

            ProjectDirectory = projectDirectory;
            RootDirectory = Microsoft.Framework.Runtime.ProjectResolver.ResolveRootDirectory(ProjectDirectory);

            // A new project resolver is required. you cannot reuse the one from the 
            // parent service provider as that will be for the parent context.
            ProjectResolver = new ProjectResolver(ProjectDirectory, RootDirectory);

            var referenceAssemblyDependencyResolver = new ReferenceAssemblyDependencyResolver(new FrameworkReferenceResolver());

            // Need to pass through package directory incase you download a package from the gallary, this needs to know about it
            var nuGetDependencyProvider = new NuGetDependencyResolver(new PackageRepository(
                NuGetDependencyResolver.ResolveRepositoryPath(RootDirectory)));
            var gacDependencyResolver = new GacDependencyResolver();
            var projectDepencyProvider = new ProjectReferenceDependencyProvider(ProjectResolver);
            var unresolvedDependencyProvider = new UnresolvedDependencyProvider();

            var projectLockJsonPath = Path.Combine(ProjectDirectory, LockFileFormat.LockFileName);
            if (File.Exists(projectLockJsonPath)) {
                var lockFileFormat = new LockFileFormat();
                var lockFile = lockFileFormat.Read(projectLockJsonPath);
                nuGetDependencyProvider.ApplyLockFile(lockFile);
            }

            DependencyWalker = new DependencyWalker(new IDependencyProvider[] {
                projectDepencyProvider,
                nuGetDependencyProvider,
                referenceAssemblyDependencyResolver,
                gacDependencyResolver,
                unresolvedDependencyProvider
            });

            LibraryExportProvider = new CompositeLibraryExportProvider(new ILibraryExportProvider[] {
                new ModuleProjectLibraryExportProvider(
                    ProjectResolver, serviceProvider),
                referenceAssemblyDependencyResolver,
                gacDependencyResolver,
                nuGetDependencyProvider
            });
        }
        public string RootDirectory { get; }

        public string ProjectDirectory { get; }

        public DependencyWalker DependencyWalker { get; private set; }
        public ILibraryExportProvider LibraryExportProvider { get; private set; }
        public IProjectResolver ProjectResolver { get; }
    }
}