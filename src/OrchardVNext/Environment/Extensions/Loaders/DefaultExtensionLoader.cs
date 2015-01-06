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
        private readonly ILibraryManager _libraryManager;
        private readonly ICompilerOptionsProvider _compilerOptionsProvider;
        private readonly IFileWatcher _watcher;


        public DefaultExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            IApplicationEnvironment applicationEnvironment,
            IEnumerable<IExtensionFolders> extensionFolders,
            IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
            ILibraryManager libraryManager,
            ICompilerOptionsProvider compilerOptionsProvider,
            IFileWatcher watcher) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _extensionFolders = extensionFolders;
            _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
            _libraryManager = libraryManager;
            _compilerOptionsProvider = compilerOptionsProvider;
            _watcher = watcher;

        }

        public Assembly Load(Project project) {
            return null;
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
               _serviceProvider);

            var library = moduleContext.DependencyWalker.Libraries.First(i => i.Identity.Name == project.Name);
            
            var libInfo = new LibraryInformation(library);

            var exports = ProjectExportProviderHelper.GetExportsRecursive(
                cache,
                new LibraryManagerWrapper(_libraryManager, libInfo),
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

    public class LibraryManagerWrapper : ILibraryManager {
        private readonly ILibraryManager _libraryManager;
        private readonly ILibraryInformation _additionalLibrary;

        public LibraryManagerWrapper(ILibraryManager libraryManager, ILibraryInformation additionalLibrary) {
            _libraryManager = libraryManager;
            _additionalLibrary = additionalLibrary;
        }

        public ILibraryExport GetAllExports(string name) {
            return _libraryManager.GetAllExports(name);
        }

        public ILibraryExport GetAllExports(string name, string aspect) {
            return _libraryManager.GetAllExports(name, aspect);
        }

        public IEnumerable<ILibraryInformation> GetLibraries() {
            return _libraryManager.GetLibraries();
        }

        public IEnumerable<ILibraryInformation> GetLibraries(string aspect) {
            return _libraryManager.GetLibraries(aspect);
        }

        public ILibraryExport GetLibraryExport(string name) {
            return _libraryManager.GetLibraryExport(name);
        }

        public ILibraryExport GetLibraryExport(string name, string aspect) {
            return _libraryManager.GetLibraryExport(name, aspect);
        }

        public ILibraryInformation GetLibraryInformation(string name) {
            var info = _libraryManager.GetLibraryInformation(name);
            if (info != null)
                return info;

            if (_additionalLibrary.Name == name)
                return _additionalLibrary;

            return null;
        }

        public ILibraryInformation GetLibraryInformation(string name, string aspect) {
            var info = _libraryManager.GetLibraryInformation(name, aspect);
            if (info != null)
                return info;

            if (_additionalLibrary.Name == name)
                return _additionalLibrary;

            return null;
        }

        public IEnumerable<ILibraryInformation> GetReferencingLibraries(string name) {
            return _libraryManager.GetReferencingLibraries(name);
        }

        public IEnumerable<ILibraryInformation> GetReferencingLibraries(string name, string aspect) {
            return _libraryManager.GetReferencingLibraries(name, aspect);
        }
    }
}