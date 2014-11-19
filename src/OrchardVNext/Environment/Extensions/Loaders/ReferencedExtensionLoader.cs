using System.IO;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.Dependencies;
using OrchardVNext.FileSystems.VirtualPath;
using System.Reflection;

namespace OrchardVNext.Environment.Extensions.Loaders {
    /// <summary>
    /// Load an extension by looking through the BuildManager referenced assemblies
    /// </summary>
    public class ReferencedExtensionLoader : ExtensionLoaderBase {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IBuildManager _buildManager;
        private readonly IAssemblyLoader _assemblyLoader;

        public ReferencedExtensionLoader(IDependenciesFolder dependenciesFolder, 
            IVirtualPathProvider virtualPathProvider, 
            IBuildManager buildManager, 
            IAssemblyLoader assemblyLoader )
            : base(dependenciesFolder) {

            _virtualPathProvider = virtualPathProvider;
            _buildManager = buildManager;
            _assemblyLoader = assemblyLoader;
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
            //Microsoft.Framework.Runtime.Loader.AssemblyLoadContextFactory;
            var assemblyPath = _virtualPathProvider.Combine("~/bin", descriptor.Id + ".dll");

            if (!_virtualPathProvider.FileExists(assemblyPath))
                return null;

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                Loader = this,
                Priority = 100, // Higher priority because assemblies in ~/bin always take precedence
                VirtualPath = assemblyPath,
                VirtualPathDependencies = new[] { assemblyPath },
            };
        }

        protected override ExtensionEntry LoadWorker(ExtensionDescriptor descriptor) {
            if (Disabled)
                return null;

            var assemblyPath = _virtualPathProvider.Combine("~/bin", descriptor.Id + ".dll");

            if (!_virtualPathProvider.FileExists(assemblyPath))
                return null;

            var assembly = _assemblyLoader.Load(assemblyPath);

            Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.GetExportedTypes()
            };
        }
    }
}
