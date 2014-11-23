using System.IO;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.Dependencies;
using OrchardVNext.FileSystems.VirtualPath;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.PackageManager;
using System.Linq;
using System;

namespace OrchardVNext.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking through the BuildManager referenced assemblies
    /// </summary>
    public class ReferencedExtensionLoader : ExtensionLoaderBase {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILibraryManager _libraryManager;
        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;

        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder,
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            ILibraryManager libraryManager,
            IAssemblyLoadContextFactory assemblyLoadContextFactory)
            : base(dependenciesFolder) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _libraryManager = libraryManager;
            _assemblyLoadContextFactory = assemblyLoadContextFactory;

        }

        public bool Disabled { get; set; }

        public override int Order { get { return 20; } }

        public override void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
            DeleteAssembly(ctx, extension.Id);
        }

        public override void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
            DeleteAssembly(ctx, dependency.Name);
        }

        private void DeleteAssembly(ExtensionLoadingContext ctx, string moduleName) {
            var assemblyPath = _virtualPathProvider.Combine("~/bin", moduleName + ".dll");
            if (_virtualPathProvider.FileExists(assemblyPath)) {
                ctx.DeleteActions.Add(
                    () => {
                        Logger.Information("ExtensionRemoved: Deleting assembly \"{0}\" from bin directory (AppDomain will restart)", moduleName);
                        File.Delete(_virtualPathProvider.MapPath(assemblyPath));
                    });
                ctx.RestartAppDomain = true;
            }
        }

        public override ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            var plocation = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(descriptor.Location, descriptor.Id));
            Project project = null;
            Project.TryGetProject(plocation, out project);

            var buildOptions = new BuildOptions();
            buildOptions.ProjectDir = project.ProjectDirectory;
            buildOptions.OutputDir = Path.Combine(project.ProjectDirectory, "bin");
            buildOptions.TargetFrameworks = project.GetTargetFrameworks().Select(x => x.FrameworkName.FullName).ToArray();
            buildOptions.Configurations = project.GetConfigurations().ToList();
            buildOptions.Reports = CreateReports(false, false);

            var buildManager = new BuildManager(_serviceProvider, buildOptions);
            try {
                // Not working when F5 from VS
                if (!buildManager.Build()) {
                    return null;
                }
            }
            catch (Exception) {
                Logger.Warning("project not built - you probably F5 from VS right!?");
            }

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                Loader = this,
                Priority = 100, // Higher priority because assemblies in ~/bin always take precedence
                VirtualPath = project.ProjectDirectory,
                VirtualPathDependencies = new[] { project.ProjectDirectory },
            };
        }

        private Reports CreateReports(bool verbose, bool quiet) {
            return new Reports {
                Information = new FakeReport(),
                Error = new FakeReport(),
                Quiet = new FakeReport(),
                Verbose = new FakeReport()
            };
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            var a = _assemblyLoadContextFactory.Create();//.Load(project.Name);
            
            var assembly = a.Load(descriptor.Id);

            Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.ExportedTypes
            };
        }
    }
}
