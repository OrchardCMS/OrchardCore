using System;
using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.Environment.Extensions.Models;
using System.Linq;
using Microsoft.Framework.Runtime;
using OrchardVNext.FileSystems.VirtualPath;
using System.Runtime.Versioning;
using OrchardVNext.Environment.Extensions.Folders;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class DefaultExtensionLoader : IExtensionLoader {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IEnumerable<IExtensionFolders> _extensionFolders;

        public DefaultExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            IAssemblyLoadContextFactory assemblyLoadContextFactory,
            IApplicationEnvironment applicationEnvironment,
            IEnumerable<IExtensionFolders> extensionFolders) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _assemblyLoadContextFactory = assemblyLoadContextFactory;
            _applicationEnvironment = applicationEnvironment;
            _extensionFolders = extensionFolders;
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

            var assembly = Load(project);

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

        public IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor extensionDescriptor) {
            return Enumerable.Empty<ExtensionReferenceProbeEntry>();
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }

        public Assembly Load(Project project) {
            var target = new LibraryKey {
                Name = project.Name,
                Configuration = _applicationEnvironment.Configuration,
                TargetFramework = project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName
            };

            var accessor = (ICacheContextAccessor)_serviceProvider.GetService(typeof(ICacheContextAccessor));

            ProjectHostContext hostContext = new ProjectHostContext(
                _serviceProvider,
                project.ProjectDirectory,
                null,
                _extensionFolders.SelectMany(ef => ef.SearchPaths).ToArray(),
                target.Configuration,
                target.TargetFramework,
                new Cache(accessor),
                accessor,
                new NamedCacheDependencyProvider()
                );

            hostContext.DependencyWalker.Walk(project.Name, project.Version, target.TargetFramework);

            var provider = (ILibraryExportProvider)hostContext.ServiceProvider.GetService(typeof(ILibraryExportProvider));

            ILibraryExport export = provider.GetLibraryExport(target);

            if (export == null) {
                return null;
            }

            foreach (var projectReference in export.MetadataReferences.OfType<IMetadataProjectReference>()) {
                if (string.Equals(projectReference.Name, project.Name, StringComparison.OrdinalIgnoreCase)) {
                    return projectReference.Load(_assemblyLoadContextFactory.Create());
                }
            }

            return null;
        }
    }

    internal class LibraryKey : ILibraryKey {
        public string Name { get; set; }
        public FrameworkName TargetFramework { get; set; }
        public string Configuration { get; set; }
        public string Aspect { get; set; }
    }
}