using System;
using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.Dependencies;
using System.Linq;
using Microsoft.Framework.Runtime;
using OrchardVNext.FileSystems.VirtualPath;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class DefaultExtensionLoader : IExtensionLoader {

        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILibraryManager _libraryManager;
        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;

        public DefaultExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            ILibraryManager libraryManager,
            IAssemblyLoadContextFactory assemblyLoadContextFactory) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _libraryManager = libraryManager;
            _assemblyLoadContextFactory = assemblyLoadContextFactory;

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

        public void ExtensionRemoved(ExtensionLoadingContext ctx, DependencyDescriptor dependency) {
        }

        public IEnumerable<ExtensionCompilationReference> GetCompilationReferences(DependencyDescriptor dependency) {
            return Enumerable.Empty<ExtensionCompilationReference>();
        }

        public IEnumerable<string> GetVirtualPathDependencies(DependencyDescriptor dependency) {
            return Enumerable.Empty<string>();
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
            
            var assembly = Assembly.Load(new AssemblyName(project.Name));

            Logger.Information("Loaded referenced extension \"{0}\": assembly name=\"{1}\"", descriptor.Name, assembly.FullName);

            return new ExtensionEntry {
                Descriptor = descriptor,
                Assembly = assembly,
                ExportedTypes = assembly.ExportedTypes
            };
        }

        public Assembly LoadReference(DependencyReferenceDescriptor reference) {
            return null;
        }

        public ExtensionProbeEntry Probe(ExtensionDescriptor descriptor) {
            var plocation = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(descriptor.Location, descriptor.Id));
            Project project = null;
            if (!Project.TryGetProject(plocation, out project)) {
                return null;
            }

            return new ExtensionProbeEntry {
                Descriptor = descriptor,
                Loader = this,
                Priority = 100, // Higher priority because assemblies in ~/bin always take precedence
                VirtualPath = project.ProjectDirectory,
                VirtualPathDependencies = new[] { project.ProjectDirectory },
            };
        }

        public IEnumerable<ExtensionReferenceProbeEntry> ProbeReferences(ExtensionDescriptor extensionDescriptor) {
            return Enumerable.Empty<ExtensionReferenceProbeEntry>();
        }

        public void ReferenceActivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }

        public void ReferenceDeactivated(ExtensionLoadingContext context, ExtensionReferenceProbeEntry referenceEntry) {
        }
    }
}