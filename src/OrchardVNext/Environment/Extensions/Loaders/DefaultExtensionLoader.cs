using System;
using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.Environment.Extensions.Models;
using Microsoft.Framework.Runtime;
using OrchardVNext.FileSystems.VirtualPath;
using System.Runtime.Versioning;
using OrchardVNext.Environment.Extensions.Folders;

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
        private readonly IAssemblyLoaderContainer _loaderContainer;


        public DefaultExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            IApplicationEnvironment applicationEnvironment,
            IEnumerable<IExtensionFolders> extensionFolders,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            IOrchardLibraryManager orchardLibraryManager,
            ICompilerOptionsProvider compilerOptionsProvider,
            IFileWatcher watcher,
            IAssemblyLoaderContainer container) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _extensionFolders = extensionFolders;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _orchardLibraryManager = orchardLibraryManager;
            _compilerOptionsProvider = compilerOptionsProvider;
            _watcher = watcher;
            _loaderContainer = container;

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

            var plocation = _virtualPathProvider.MapPath(descriptor.Location);

            using (_loaderContainer.AddLoader(new ExtensionAssemblyLoader(plocation, _serviceProvider))) {
                var assembly = Assembly.Load(new AssemblyName(descriptor.Id));

                Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);


                return new ExtensionEntry {
                    Descriptor = descriptor,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes
                };
            }
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