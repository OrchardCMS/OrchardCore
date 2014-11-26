using System;
using System.Collections.Generic;
using System.Reflection;
using OrchardVNext.Environment.Extensions.Models;
using OrchardVNext.FileSystems.Dependencies;
using System.Linq;
using Microsoft.Framework.Runtime;
using OrchardVNext.FileSystems.VirtualPath;
using System.Runtime.Versioning;
using Microsoft.Framework.Runtime.Roslyn;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using NuGet;
using System.Reflection.Metadata;
using OrchardVNext.Environment.Extensions.Loaders;
using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class DefaultExtensionLoader : IExtensionLoader {
        private readonly IVirtualPathProvider _virtualPathProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILibraryManager _libraryManager;
        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;
        private readonly IAssemblyLoadContextAccessor _loadContextAccessor;
        private readonly IAssemblyLoaderContainer _loaderContainer;
        private readonly IProjectResolver _projectResolver;
        private readonly IApplicationEnvironment _applicationEnvironment;

        public DefaultExtensionLoader(
            IVirtualPathProvider virtualPathProvider,
            IServiceProvider serviceProvider,
            ILibraryManager libraryManager,
            IAssemblyLoadContextFactory assemblyLoadContextFactory,
            IAssemblyLoaderContainer container,
            IAssemblyLoadContextAccessor accessor,
            IProjectResolver projectResovler,
                                     IApplicationEnvironment applicationEnvironment,
                                     IEnumerable<IDependencyProvider> p) {

            _virtualPathProvider = virtualPathProvider;
            _serviceProvider = serviceProvider;
            _libraryManager = libraryManager;
            _assemblyLoadContextFactory = assemblyLoadContextFactory;
            _loaderContainer = container;
            _loadContextAccessor = accessor;
            _projectResolver = projectResovler;
            _applicationEnvironment = applicationEnvironment;
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

            var loadContext = _loadContextAccessor.GetLoadContext(typeof(DefaultExtensionLoader).GetTypeInfo().Assembly);

            var assembly = Load(project.Name);

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

        public Assembly Load(string name) {
            Logger.Information("Im in!!!");
            IAssemblyLoadContext loadContext = _loadContextAccessor.GetLoadContext(typeof(DefaultExtensionLoader).GetTypeInfo().Assembly);

            return Load(name, loadContext);
        }

        public Assembly Load(string name, IAssemblyLoadContext loadContext) {
            // An assembly name like "MyLibrary!alternate!more-text"
            // is parsed into:
            // name == "MyLibrary"
            // aspect == "alternate"
            // and the more-text may be used to force a recompilation of an aspect that would
            // otherwise have been cached by some layer within Assembly.Load

            string aspect = null;
            var parts = name.Split(new[] { '!' }, 3);
            if (parts.Length != 1) {
                name = parts[0];
                aspect = parts[1];
            }

            Project project;
            if (!_projectResolver.TryResolveProject(name, out project)) {
                return null;
            }

            var target = new LibraryKey {
                Name = name,
                Configuration = _applicationEnvironment.Configuration,
                TargetFramework = project.GetTargetFramework(_applicationEnvironment.RuntimeFramework).FrameworkName
            };

            var accessor = (ICacheContextAccessor)_serviceProvider.GetService(typeof(ICacheContextAccessor));

            ProjectHostContext hostContext = new ProjectHostContext(
                _serviceProvider,
                project.ProjectDirectory,
                null,
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
                if (string.Equals(projectReference.Name, name, StringComparison.OrdinalIgnoreCase)) {
                    return projectReference.Load(_assemblyLoadContextFactory.Create());
                }
            }

            return null;
        }

        private class Node {
            public ILibraryInformation Library { get; set; }

            public Node Parent { get; set; }
        }


    }

    internal class LibraryKey : ILibraryKey {
        public string Name { get; set; }
        public FrameworkName TargetFramework { get; set; }
        public string Configuration { get; set; }
        public string Aspect { get; set; }
    }


}