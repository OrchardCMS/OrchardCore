using System;
using System.Runtime.Versioning;
using Microsoft.Dnx.Runtime;
using Microsoft.Dnx.Runtime.Infrastructure;
using NuGet;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class ModuleLoaderContext {
        public ModuleLoaderContext(
            IServiceProvider hostServices,
            string projectDirectory, 
            string configuration, 
            FrameworkName targetFramework) {

            var applicationHostContext = new ApplicationHostContext(
                hostServices: hostServices,
                projectDirectory: projectDirectory,
                packagesDirectory: null,
                configuration: configuration,
                targetFramework: targetFramework);
            
            DependencyWalker = applicationHostContext.DependencyWalker;
            FrameworkName = targetFramework;
            ServiceProvider =  applicationHostContext.ServiceProvider;
            AssemblyLoadContextFactory = applicationHostContext.AssemblyLoadContextFactory;
            LibraryManager = applicationHostContext.LibraryManager;
            Project = applicationHostContext.Project;
            ProjectGraphProvider = applicationHostContext.ProjectGraphProvider;
            NuGetDependencyProvider = applicationHostContext.NuGetDependencyProvider;
            
            Walk(Project.Name, Project.Version);
        }

        public DependencyWalker DependencyWalker { get; set; }
        public FrameworkName FrameworkName { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public IAssemblyLoadContextFactory AssemblyLoadContextFactory { get; set; }
        public LibraryManager LibraryManager { get; set; }
        public Project Project { get; set; }
        public IProjectGraphProvider ProjectGraphProvider { get; set; }
        public NuGetDependencyResolver NuGetDependencyProvider { get; set; }


        private void Walk(string projectName, SemanticVersion projectVersion) {
            DependencyWalker.Walk(projectName, projectVersion, FrameworkName);
        }
    }
}