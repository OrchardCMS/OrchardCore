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
                                     IApplicationEnvironment applicationEnvironment) {

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

            //var loadContext = _loadContextAccessor.GetLoadContext(typeof(DefaultExtensionLoader).GetTypeInfo().Assembly);

            var loader = new ProjectAssemblyLoader(_projectResolver, _loadContextAccessor, _libraryManager, _serviceProvider, _applicationEnvironment, _assemblyLoadContextFactory);
            var assembly = loader.Load(project.Name);

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

    public class ProjectAssemblyLoader : IAssemblyLoader {
        private readonly IProjectResolver _projectResolver;
        private readonly ILibraryManager _libraryManager;
        private readonly IAssemblyLoadContextAccessor _loadContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApplicationEnvironment _applicationEnvironment;
        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;

        public ProjectAssemblyLoader(IProjectResolver projectResovler,
                                     IAssemblyLoadContextAccessor loadContextAccessor,
                                     ILibraryManager libraryManager,
                                     IServiceProvider serviceProvider,
                                     IApplicationEnvironment applicationEnvironment,
                                     IAssemblyLoadContextFactory assemblyLoadContextFactory) {
            _projectResolver = projectResovler;
            _loadContextAccessor = loadContextAccessor;
            _libraryManager = libraryManager;
            _serviceProvider = serviceProvider;
            _applicationEnvironment = applicationEnvironment;
            _assemblyLoadContextFactory = assemblyLoadContextFactory;
        }

        public Assembly Load(string name) {
            IAssemblyLoadContext loadContext = _loadContextAccessor.GetLoadContext(typeof(ProjectAssemblyLoader).GetTypeInfo().Assembly);

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

            //var references = new Dictionary<string, IMetadataReference>(StringComparer.OrdinalIgnoreCase);
            //var sourceReferences = new Dictionary<string, ISourceReference>(StringComparer.OrdinalIgnoreCase);

            //// Walk the dependency tree and resolve the library export for all references to this project
            //var stack = new Queue<Node>();
            //var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //var node = new Node {
            //    Library = new LibraryInformation(new LibraryDescription {
            //        Dependencies = project.Dependencies,
            //        Framework = target.TargetFramework,
            //        Identity = new Library {
            //            IsGacOrFrameworkReference = false,
            //            Name = target.Name,
            //            Version = project.Version
            //        },
            //        Path = project.ProjectDirectory,
            //        LoadableAssemblies = Enumerable.Empty<string>()
            //    })
            //};

            //var p = _libraryManager.GetLibraryInformation(project.Name, null);



            var exportProvider = new OrchardLibraryExportProvider(
                ((IProjectResolver)_serviceProvider.GetService(typeof(IProjectResolver))),
            _serviceProvider,
            (IProjectReferenceProvider)_serviceProvider.GetService(typeof(IProjectReferenceProvider)));

            var export = exportProvider.GetLibraryExport(target);

            //var export = new LibraryExport(
            //    references.Values.ToList(),
            //    sourceReferences.Values.ToList());

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

    public class OrchardCompositeLibraryExportProvider : ILibraryExportProvider {
        private readonly IEnumerable<ILibraryExportProvider> _libraryExporters;

        public OrchardCompositeLibraryExportProvider(IEnumerable<ILibraryExportProvider> libraryExporters) {
            _libraryExporters = libraryExporters;
        }

        public ILibraryExport GetLibraryExport(ILibraryKey target) {
            return _libraryExporters.Select(r => r.GetLibraryExport(target))
                                             .FirstOrDefault(export => export != null);
        }
    }


    public class OrchardLibraryExportProvider : ILibraryExportProvider {
        private readonly IProjectResolver _projectResolver;
        private readonly IServiceProvider _serviceProvider;
        private readonly IProjectReferenceProvider _projectReferenceProvider;

        public OrchardLibraryExportProvider(IProjectResolver projectResolver,
                                            IServiceProvider serviceProvider,
                                            IProjectReferenceProvider projectReferenceProvider) {
            _projectResolver = projectResolver;
            _serviceProvider = serviceProvider;
            _projectReferenceProvider = projectReferenceProvider;
        }
        public ILibraryExport GetLibraryExport(ILibraryKey target) {
            Project project;
            // Can't find a project file with the name so bail
            if (!_projectResolver.TryResolveProject(target.Name, out project)) {
                return null;
            }

            Trace.TraceInformation("[{0}]: GetLibraryExport({1}, {2}, {3}, {4})", GetType().Name, target.Name, target.TargetFramework, target.Configuration, target.Aspect);

            var targetFrameworkInformation = project.GetTargetFramework(target.TargetFramework);

            // This is the target framework defined in the project. If there were no target frameworks
            // defined then this is the targetFramework specified
            if (targetFrameworkInformation.FrameworkName != null) {
                target = target.ChangeTargetFramework(targetFrameworkInformation.FrameworkName);
            }

            var key = Tuple.Create(
                target.Name,
                target.TargetFramework,
                target.Configuration,
                target.Aspect);

            var cache = (ICache)_serviceProvider.GetService(typeof(ICache));

            return cache.Get<ILibraryExport>(key, ctx => {
                // Get the composite library export provider
                var exportProvider = (ILibraryExportProvider)_serviceProvider.GetService(typeof(ILibraryExportProvider));
                var libraryManager = (ILibraryManager)_serviceProvider.GetService(typeof(ILibraryManager));

                var metadataReferences = new List<IMetadataReference>();
                var sourceReferences = new List<ISourceReference>();

                if (!string.IsNullOrEmpty(targetFrameworkInformation.AssemblyPath)) {
                    var assemblyPath = ResolvePath(project, target.Configuration, targetFrameworkInformation.AssemblyPath);
                    var pdbPath = ResolvePath(project, target.Configuration, targetFrameworkInformation.PdbPath);

                    metadataReferences.Add(new CompiledProjectMetadataReference(project, assemblyPath, pdbPath));
                }
                else {
                    // Find the default project exporter
                    var projectReferenceProvider = _projectReferenceProvider;

                    Trace.TraceInformation("[{0}]: GetProjectReference({1}, {2}, {3}, {4})", project.LanguageServices.ProjectReferenceProvider.TypeName, target.Name, target.TargetFramework, target.Configuration, target.Aspect);

                    // Get the exports for the project dependencies
                    var projectExport = new Lazy<ILibraryExport>(() => OrchardProjectExportProviderHelper.GetExportsRecursive(
                        project,
                        cache,
                        libraryManager,
                        exportProvider,
                        target,
                        dependenciesOnly: true));

                    // Resolve the project export
                    IMetadataProjectReference projectReference = projectReferenceProvider.GetProjectReference(
                        project,
                        target,
                        () => projectExport.Value,
                        metadataReferences);

                    metadataReferences.Add(projectReference);

                    // Shared sources
                    foreach (var sharedFile in project.SharedFiles) {
                        sourceReferences.Add(new SourceFileReference(sharedFile));
                    }
                }

                return new LibraryExport(metadataReferences, sourceReferences);
            });
        }

        private static string ResolvePath(Project project, string configuration, string path) {
            if (string.IsNullOrEmpty(path)) {
                return null;
            }

            if (Path.DirectorySeparatorChar == '/') {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }

            path = path.Replace("{configuration}", configuration);

            return Path.Combine(project.ProjectDirectory, path);
        }
    }

    internal static class DictionaryExtensions {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory) {
            lock (dictionary) {
                TValue value;
                if (!dictionary.TryGetValue(key, out value)) {
                    value = factory(key);
                    dictionary[key] = value;
                }

                return value;
            }
        }
    }

    /// <summary>
    /// Summary description for LibraryKey
    /// </summary>
    internal static class LibraryKeyExtensions {
        public static ILibraryKey ChangeName(this ILibraryKey target, string name) {
            return new LibraryKey {
                Name = name,
                TargetFramework = target.TargetFramework,
                Configuration = target.Configuration,
                Aspect = target.Aspect,
            };
        }

        public static ILibraryKey ChangeTargetFramework(this ILibraryKey target, FrameworkName targetFramework) {
            return new LibraryKey {
                Name = target.Name,
                TargetFramework = targetFramework,
                Configuration = target.Configuration,
                Aspect = target.Aspect,
            };
        }

        public static ILibraryKey ChangeAspect(this ILibraryKey target, string aspect) {
            return new LibraryKey {
                Name = target.Name,
                TargetFramework = target.TargetFramework,
                Configuration = target.Configuration,
                Aspect = aspect,
            };
        }
    }

    public static class OrchardProjectExportProviderHelper {
        public static ILibraryExport GetExportsRecursive(
            Project project,
            ICache cache,
            ILibraryManager manager,
            ILibraryExportProvider libraryExportProvider,
            ILibraryKey target,
            bool dependenciesOnly) {
            return GetExportsRecursive(project, cache, manager, libraryExportProvider, target, libraryInformation => {
                if (dependenciesOnly) {
                    return !string.Equals(target.Name, libraryInformation.Name);
                }

                return true;
            });
        }

        public static ILibraryExport GetExportsRecursive(
            Project project,
            ICache cache,
            ILibraryManager manager,
            ILibraryExportProvider libraryExportProvider,
            ILibraryKey target,
            Func<ILibraryInformation, bool> include) {
            var dependencyStopWatch = Stopwatch.StartNew();
            Trace.TraceInformation("[{0}]: Resolving references for '{1}' {2}", typeof(OrchardProjectExportProviderHelper).Name, target.Name, target.Aspect);

            var references = new Dictionary<string, IMetadataReference>(StringComparer.OrdinalIgnoreCase);
            var sourceReferences = new Dictionary<string, ISourceReference>(StringComparer.OrdinalIgnoreCase);

            // Walk the dependency tree and resolve the library export for all references to this project
            var stack = new Queue<Node>();
            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var rootNode = new Node {
                Library = new LibraryInformation(new LibraryDescription {
                    Dependencies = project.Dependencies,
                    Framework = target.TargetFramework,
                    Identity = new Library {
                        IsGacOrFrameworkReference = false,
                        Name = target.Name,
                        Version = project.Version
                    },
                    Path = project.ProjectDirectory,
                    LoadableAssemblies = Enumerable.Empty<string>()
                })
            };

            stack.Enqueue(rootNode);

            while (stack.Count > 0) {
                var node = stack.Dequeue();

                // Skip it if we've already seen it
                if (!processed.Add(node.Library.Name)) {
                    continue;
                }

                if (include(node.Library)) {
                    var libraryExport = libraryExportProvider.GetLibraryExport(target
                        .ChangeName(node.Library.Name)
                        .ChangeAspect(null));

                    if (libraryExport == null) {
                        // TODO: Failed to resolve dependency so do something useful
                        Trace.TraceInformation("[{0}]: Failed to resolve dependency '{1}'", typeof(OrchardProjectExportProviderHelper).Name, node.Library.Name);
                    }
                    else {
                        if (node.Parent == rootNode) {
                            // Only export sources from first level dependencies
                            ProcessExport(cache, libraryExport, references, sourceReferences);
                        }
                        else {
                            // Skip source exports from anything else
                            ProcessExport(cache, libraryExport, references, sourceReferences: null);
                        }
                    }
                }

                foreach (var dependency in node.Library.Dependencies) {
                    var childNode = new Node {
                        Library = manager.GetLibraryInformation(dependency, null),
                        Parent = node
                    };

                    stack.Enqueue(childNode);
                }
            }

            dependencyStopWatch.Stop();
            Trace.TraceInformation("[{0}]: Resolved {1} references for '{2}' in {3}ms",
                                  typeof(OrchardProjectExportProviderHelper).Name,
                                  references.Count,
                                  target.Name,
                                  dependencyStopWatch.ElapsedMilliseconds);

            return new LibraryExport(
                references.Values.ToList(),
                sourceReferences.Values.ToList());
        }

        private static void ProcessExport(ICache cache,
                                          ILibraryExport export,
                                          IDictionary<string, IMetadataReference> metadataReferences,
                                          IDictionary<string, ISourceReference> sourceReferences) {
            var references = new List<IMetadataReference>(export.MetadataReferences);

            ExpandEmbeddedReferences(cache, references);

            foreach (var reference in references) {
                metadataReferences[reference.Name] = reference;
            }

            if (sourceReferences != null) {
                foreach (var sourceReference in export.SourceReferences) {
                    sourceReferences[sourceReference.Name] = sourceReference;
                }
            }
        }

        private static void ExpandEmbeddedReferences(ICache cache, IList<IMetadataReference> references) {
            var otherReferences = new List<IMetadataReference>();

            foreach (var reference in references) {
                var fileReference = reference as IMetadataFileReference;

                if (fileReference != null &&
                    string.Equals(Path.GetExtension(fileReference.Path), ".dll", StringComparison.OrdinalIgnoreCase)) {
                    // We don't use the exact path since that might clash with another key
                    //var key = "ANI_" + fileReference.Path;

                    //var embeddedRefs = cache.Get<IList<IMetadataEmbeddedReference>>(key, ctx => {
                    //    ctx.Monitor(new FileWriteTimeCacheDependency(fileReference.Path));

                    //    using (var fileStream = File.OpenRead(fileReference.Path))
                    //    using (var reader = new PEReader(fileStream)) {
                    //        return reader.GetEmbeddedReferences();
                    //    }
                    //});

                    //otherReferences.AddRange(embeddedRefs);
                }
            }

            references.AddRange(otherReferences);
        }

        private class Node {
            public ILibraryInformation Library { get; set; }

            public Node Parent { get; set; }
        }
    }
}