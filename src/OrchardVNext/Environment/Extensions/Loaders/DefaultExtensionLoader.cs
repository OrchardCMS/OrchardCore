using System;
using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.Environment.Extensions.Models;
using System.Linq;
using Microsoft.Framework.Runtime;
using OrchardVNext.FileSystems.VirtualPath;
using System.Runtime.Versioning;
using OrchardVNext.Environment.Extensions.Folders;
using Microsoft.Framework.Runtime.Roslyn;
using Microsoft.Framework.Runtime.Loader;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class DefaultExtensionLoader : IExtensionLoader {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IEnumerable<IExtensionFolders> _extensionFolders;
        private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
        private readonly IOrchardLibraryManager _orchardLibraryManager;
        private readonly ICompilerOptionsProvider _compilerOptionsProvider;
        private readonly IFileWatcher _watcher;


        public DefaultExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            IApplicationEnvironment applicationEnvironment,
            IEnumerable<IExtensionFolders> extensionFolders,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IOrchardLibraryManager orchardLibraryManager,
            ICompilerOptionsProvider compilerOptionsProvider,
            IFileWatcher watcher) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _extensionFolders = extensionFolders;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _orchardLibraryManager = orchardLibraryManager;
            _compilerOptionsProvider = compilerOptionsProvider;
            _watcher = watcher;

        }

        public string Name { get { return this.GetType().Name; } }

        public int Order {
            get {
                return 1;
            }
        }

        public void ExtensionActivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public void ExtensionDeactivated(ExtensionLoadingContext ctx, ExtensionDescriptor extension) {
        }

        public bool IsCompatibleWithModuleReferences(ExtensionDescriptor extension, IEnumerable<ExtensionProbeEntry> references) {
            return true;
        }

        public ExtensionEntry Load(ExtensionDescriptor descriptor) {
            var plocation = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(descriptor.Location, descriptor.Id));
            Project project = null;
            if (!Project.TryGetProject(plocation, out project)) {
                return null;
            }

            var cache = (ICache)_serviceProvider.GetService(typeof(ICache));

            var target = new LibraryKey {
                Name = project.Name,
                Configuration = _applicationEnvironment.Configuration,
                TargetFramework = project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName
            };

            ModuleLoaderContext moduleContext = new ModuleLoaderContext(
                _serviceProvider, 
                project.ProjectDirectory,
                cache);

            moduleContext.DependencyWalker.Walk(project.Name, project.Version, target.TargetFramework);

            var cacheContextAccessor = (ICacheContextAccessor)_serviceProvider.GetService(typeof(ICacheContextAccessor));
            var loadContextFactory = (IAssemblyLoadContextFactory)_serviceProvider.GetService(typeof(IAssemblyLoadContextFactory)) ?? new AssemblyLoadContextFactory(_serviceProvider);

            var compiler = new InternalRoslynCompiler(
               cache,
               cacheContextAccessor,
               new NamedCacheDependencyProvider(),
               loadContextFactory,
               _watcher,
               _applicationEnvironment,
               _serviceProvider);

            _orchardLibraryManager.AddAdditionalRegistrations(moduleContext.DependencyWalker.Libraries);

            var exports = ProjectExportProviderHelper.GetExportsRecursive(
                cache,
                _orchardLibraryManager,
                moduleContext.LibraryExportProvider,
                target,
                true);

            var compliationContext = compiler.CompileProject(project, 
                target,
                exports.MetadataReferences,
                exports.SourceReferences,
                Enumerable.Empty<IMetadataReference>().ToList());

            var roslynProjectReference = new RoslynProjectReference(compliationContext);

            var loadContext = _assemblyLoadContextAccessor.Default;
            var assembly = roslynProjectReference.Load(loadContext);

            Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.ExportedTypes
            };
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            return null;
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }
    }

    internal class LibraryKey : ILibraryKey {
        public string Name { get; set; }
        public FrameworkName TargetFramework { get; set; }
        public string Configuration { get; set; }
        public string Aspect { get; set; }
    }
}