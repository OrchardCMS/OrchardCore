using System.Runtime.Versioning;
using Microsoft.Dnx.Runtime;

namespace Orchard.Environment.Extensions.Loaders
{
    public class ModuleLoaderContext
    {
        public ModuleLoaderContext(
            string projectDirectory,
            FrameworkName targetFramework)
        {
            Project.DefaultCompiler = Project.DefaultRuntimeCompiler;

            var applicationHostContext = new ApplicationHostContext
            {
                ProjectDirectory = projectDirectory,
                TargetFramework = targetFramework
            };

            ApplicationHostContext.Initialize(applicationHostContext);

            LibraryManager = applicationHostContext.LibraryManager;
            Project = applicationHostContext.Project;
            TargetFramework = applicationHostContext.TargetFramework;
        }

        public LibraryManager LibraryManager { get; set; }
        public Project Project { get; set; }
        public FrameworkName TargetFramework { get; private set; }
    }
}