using System.Runtime.Versioning;
using Microsoft.Dnx.Runtime;

namespace OrchardVNext.Hosting.Extensions.Loaders {
    public class ModuleLoaderContext {
        public ModuleLoaderContext(
            string projectDirectory, 
            FrameworkName targetFramework) {

            var applicationHostContext = new ApplicationHostContext {
                ProjectDirectory = projectDirectory,
                TargetFramework = targetFramework
            };

            ApplicationHostContext.Initialize(applicationHostContext);

            FrameworkName = applicationHostContext.TargetFramework;
            LibraryManager = applicationHostContext.LibraryManager;
            Project = applicationHostContext.Project;
            PackagesDirectory = applicationHostContext.PackagesDirectory;
            TargetFramework = applicationHostContext.TargetFramework;
        }

        public FrameworkName FrameworkName { get; set; }
        public LibraryManager LibraryManager { get; set; }
        public Project Project { get; set; }
        public string PackagesDirectory { get; set; }
        public FrameworkName TargetFramework { get; private set; }
    }
}