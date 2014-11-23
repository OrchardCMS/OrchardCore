//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OrchardVNext.Environment.Extensions.Loaders;
//using OrchardVNext.Environment.Extensions.Models;
//using OrchardVNext.FileSystems.Dependencies;
//using OrchardVNext.FileSystems.VirtualPath;
//using OrchardVNext.Localization;
//using OrchardVNext.Logging;
//using OrchardVNext.Utility;
//using Microsoft.Framework.Runtime;
//using System.Runtime.Versioning;
//using NuGet;
//using Microsoft.Framework.Runtime.Roslyn;

//namespace OrchardVNext.Environment.Extensions {
//    public class ExtensionLoaderCoordinator : IExtensionLoaderCoordinator {
//        private readonly IServiceProvider _hostServices;
//        private readonly ICache _cache;
//        private readonly ICacheContextAccessor _cacheContextAccessor;
//        private readonly INamedCacheDependencyProvider _namedDependencyProvider;
//        private readonly IApplicationEnvironment _appEnv;
//        private readonly IExtensionManager _extensionManager;
//        private readonly IVirtualPathProvider _virtualPathProvider;
//        private readonly IAssemblyLoadContextFactory _assemblyLoadContextFactory;

//        public ExtensionLoaderCoordinator(
//IServiceProvider services,
//                                  ICache cache,
//                                  ICacheContextAccessor cacheContextAccessor,
//                                  INamedCacheDependencyProvider namedDependencyProvider,
//                                  IApplicationEnvironment applicationEnvrionment,
//            IExtensionManager extensionManager,
//            IVirtualPathProvider virtualPathProvider,
//            IAssemblyLoadContextFactory assemblyLoadContextFactory) {

//            _hostServices = services;
//            _appEnv = applicationEnvrionment;
//            _cache = cache;
//            _cacheContextAccessor = cacheContextAccessor;
//            _namedDependencyProvider = namedDependencyProvider;
//            _extensionManager = extensionManager;
//            _virtualPathProvider = virtualPathProvider;
//            _assemblyLoadContextFactory = assemblyLoadContextFactory;

//            T = NullLocalizer.Instance;
//        }

//        public Localizer T { get; set; }

//        public void SetupExtensions() {
//            Logger.Information("Start loading extensions...");

//            var context = CreateLoadingContext();

//            //// Notify all loaders about extensions removed from the web site
//            //foreach (var dependency in context.DeletedDependencies) {
//            //    Logger.Information("Extension {0} has been removed from site", dependency.Name);
//            //    foreach (var loader in _loaders) {
//            //        if (dependency.LoaderName == loader.Name) {
//            //            loader.ExtensionRemoved(context, dependency);
//            //        }
//            //    }
//            //}

//            //// For all existing extensions in the site, ask each loader if they can
//            //// load that extension.
//            //foreach (var extension in context.AvailableExtensions) {
//            //    ProcessExtension(context, extension);
//            //}

//            //// Execute all the work need by "ctx"
//            //ProcessContextCommands(context);

//            //// And finally save the new entries in the dependencies folder
//            //_dependenciesFolder.StoreDescriptors(context.NewDependencies);
//            //_extensionDependenciesManager.StoreDependencies(context.NewDependencies, desc => GetExtensionHash(context, desc));

//            Logger.Information("Done loading extensions...");
//        }

//        //private string GetExtensionHash(ExtensionLoadingContext context, DependencyDescriptor dependencyDescriptor) {
//        //    var hash = new Hash();
//        //    hash.AddStringInvariant(dependencyDescriptor.Name);

//        //    foreach (var virtualpathDependency in context.ProcessedExtensions[dependencyDescriptor.Name].VirtualPathDependencies) {
//        //        hash.AddDateTime(GetVirtualPathModificationTimeUtc(context.VirtualPathModficationDates, virtualpathDependency));
//        //    }

//        //    foreach (var reference in dependencyDescriptor.References) {
//        //        hash.AddStringInvariant(reference.Name);
//        //        hash.AddString(reference.LoaderName);
//        //        hash.AddDateTime(GetVirtualPathModificationTimeUtc(context.VirtualPathModficationDates, reference.VirtualPath));
//        //    }

//        //    return hash.Value;
//        //}

//        //private void ProcessExtension(ExtensionLoadingContext context, ExtensionDescriptor extension) {

//        //    var extensionProbes = context.AvailableExtensionsProbes.ContainsKey(extension.Id) ?
//        //        context.AvailableExtensionsProbes[extension.Id] :
//        //        Enumerable.Empty<ExtensionProbeEntry>();

//        //    // materializes the list
//        //    extensionProbes = extensionProbes.ToArray();

//        //    if (Logger.IsEnabled(LogLevel.Debug)) {
//        //        Logger.Debug("Loaders for extension \"{0}\": ", extension.Id);
//        //        foreach (var probe in extensionProbes) {
//        //            Logger.Debug("  Loader: {0}", probe.Loader.Name);
//        //            Logger.Debug("    VirtualPath: {0}", probe.VirtualPath);
//        //            Logger.Debug("    VirtualPathDependencies: {0}", string.Join(", ", probe.VirtualPathDependencies));
//        //        }
//        //    }

//        //    var moduleReferences =
//        //        context.AvailableExtensions
//        //            .Where(e =>
//        //                   context.ReferencesByModule.ContainsKey(extension.Id) &&
//        //                   context.ReferencesByModule[extension.Id].Any(r => StringComparer.OrdinalIgnoreCase.Equals(e.Id, r.Name)))
//        //            .ToList();

//        //    var processedModuleReferences =
//        //        moduleReferences
//        //        .Where(e => context.ProcessedExtensions.ContainsKey(e.Id))
//        //        .Select(e => context.ProcessedExtensions[e.Id])
//        //        .ToList();

//        //    var activatedExtension = extensionProbes.FirstOrDefault(
//        //        e => e.Loader.IsCompatibleWithModuleReferences(extension, processedModuleReferences)
//        //        );

//        //    var previousDependency = context.PreviousDependencies.FirstOrDefault(
//        //        d => StringComparer.OrdinalIgnoreCase.Equals(d.Name, extension.Id)
//        //        );

//        //    if (activatedExtension == null) {
//        //        Logger.Warning("No loader found for extension \"{0}\"!", extension.Id);
//        //    }

//        //    var references = ProcessExtensionReferences(context, activatedExtension);

//        //    foreach (var loader in _loaders) {
//        //        if (activatedExtension != null && activatedExtension.Loader.Name == loader.Name) {
//        //            Logger.Information("Activating extension \"{0}\" with loader \"{1}\"", activatedExtension.Descriptor.Id, loader.Name);
//        //            loader.ExtensionActivated(context, extension);
//        //        }
//        //        else if (previousDependency != null && previousDependency.LoaderName == loader.Name) {
//        //            Logger.Information("Deactivating extension \"{0}\" from loader \"{1}\"", previousDependency.Name, loader.Name);
//        //            loader.ExtensionDeactivated(context, extension);
//        //        }
//        //    }

//        //    if (activatedExtension != null) {
//        //        context.NewDependencies.Add(new DependencyDescriptor {
//        //            Name = extension.Id,
//        //            LoaderName = activatedExtension.Loader.Name,
//        //            VirtualPath = activatedExtension.VirtualPath,
//        //            References = references
//        //        });
//        //    }

//        //    // Keep track of which loader we use for every extension
//        //    // This will be needed for processing references from other dependent extensions
//        //    context.ProcessedExtensions.Add(extension.Id, activatedExtension);
//        //}

//        private ExtensionLoadingContext CreateLoadingContext() {
//            var availableExtensions = _extensionManager
//                .AvailableExtensions()
//                .Where(d => DefaultExtensionTypes.IsModule(d.ExtensionType) || DefaultExtensionTypes.IsTheme(d.ExtensionType))
//                .OrderBy(d => d.Id)
//                .ToList();

//            var context = _assemblyLoadContextFactory.Create();

//            availableExtensions.ForEach(x => {
//                Logger.Information("Examaning {0}", x.Id);

//                var projectPath = _virtualPathProvider.MapPath(_virtualPathProvider.Combine(x.Location, x.Id));

//                var state = DoInitialWork(projectPath, _appEnv.Configuration, false);

//                foreach (var project in state.Projects) {




//                };

//                Logger.Information("Examined {0}", x.Id);
//            });

//            return null;
//        }

//        private State DoInitialWork(string appPath, string configuration, bool triggerBuildOutputs) {
//            var state = new State {
//                Frameworks = new List<FrameworkData>(),
//                Projects = new List<ProjectInfo>()
//            };

//            Project project;
//            if (!Project.TryGetProject(appPath, out project)) {
//                throw new InvalidOperationException(string.Format("Unable to find project.json in '{0}'", appPath));
//            }

//            if (triggerBuildOutputs) {
//                // Trigger the build outputs for this project
//                _namedDependencyProvider.Trigger(project.Name + "_BuildOutputs");
//            }

//            state.Name = project.Name;
//            state.Configurations = project.GetConfigurations().ToList();
//            state.Commands = project.Commands;

//            var frameworks = new List<FrameworkName>(
//                project.GetTargetFrameworks()
//                .Select(tf => tf.FrameworkName));

//            if (!frameworks.Any()) {
//                frameworks.Add(VersionUtility.ParseFrameworkName("aspnet50"));
//            }

//            var sourcesProjectWideSources = project.SourceFiles.ToList();

//            foreach (var frameworkName in frameworks) {
//                var dependencyInfo = ResolveProjectDepencies(project, configuration, frameworkName);
//                var dependencySources = new List<string>(sourcesProjectWideSources);

//                var frameworkResolver = dependencyInfo.HostContext.FrameworkReferenceResolver;

//                var frameworkData = new FrameworkData {
//                    ShortName = VersionUtility.GetShortFrameworkName(frameworkName),
//                    FrameworkName = frameworkName.ToString(),
//                    FriendlyName = frameworkResolver.GetFriendlyFrameworkName(frameworkName),
//                    RedistListPath = frameworkResolver.GetFrameworkRedistListPath(frameworkName)
//                };

//                state.Frameworks.Add(frameworkData);

//                // Add shared files
//                foreach (var reference in dependencyInfo.ProjectReferences) {
//                    Project referencedProject;
//                    if (Project.TryGetProject(reference.Path, out referencedProject)) {
//                        dependencySources.AddRange(referencedProject.SharedFiles);
//                    }
//                }

//                var projectInfo = new ProjectInfo() {
//                    Path = appPath,
//                    Configuration = configuration,
//                    TargetFramework = frameworkData,
//                    FrameworkName = frameworkName,
//                    // TODO: This shouldn't be roslyn specific compilation options
//                    CompilationSettings = project.GetCompilationSettings(frameworkName, configuration),
//                    SourceFiles = dependencySources,
//                    DependencyInfo = dependencyInfo
//                };

//                state.Projects.Add(projectInfo);

//                if (state.ProjectSearchPaths == null) {
//                    state.ProjectSearchPaths = dependencyInfo.HostContext.ProjectResolver.SearchPaths.ToList();
//                }

//                if (state.GlobalJsonPath == null) {
//                    GlobalSettings settings;
//                    if (GlobalSettings.TryGetGlobalSettings(dependencyInfo.HostContext.RootDirectory, out settings)) {
//                        state.GlobalJsonPath = settings.FilePath;
//                    }
//                }
//            }

//            return state;
//        }

//        private DependencyInfo ResolveProjectDepencies(Project project, string configuration, FrameworkName frameworkName) {
//            //var cacheKey = Tuple.Create("DependencyInfo", project.Name, configuration, frameworkName);

//            //return _cache.Get<DependencyInfo>(cacheKey, ctx => {
//                var applicationHostContext = GetApplicationHostContext(project, configuration, frameworkName);

//            var libraryManager = applicationHostContext.LibraryManager;
//            var frameworkResolver = applicationHostContext.FrameworkReferenceResolver;

//            var info = new DependencyInfo {
//                Dependencies = new Dictionary<string, DependencyDescription>(),
//                ProjectReferences = new List<ProjectReference>(),
//                HostContext = applicationHostContext,
//                References = new List<string>(),
//                RawReferences = new Dictionary<string, byte[]>()
//            };

//            // Watch all projects for project.json changes
//            foreach (var library in applicationHostContext.DependencyWalker.Libraries) {
//                var description = CreateDependencyDescription(library);
//                info.Dependencies[description.Name] = description;

//                if (string.Equals(library.Type, "Project") &&
//                   !string.Equals(library.Identity.Name, project.Name)) {
//                    info.ProjectReferences.Add(new ProjectReference {
//                        Framework = new FrameworkData {
//                            ShortName = VersionUtility.GetShortFrameworkName(library.Framework),
//                            FrameworkName = library.Framework.ToString(),
//                            FriendlyName = frameworkResolver.GetFriendlyFrameworkName(library.Framework)
//                        },
//                        Path = library.Path
//                    });
//                }
//            }

//            var exportWithoutProjects = ProjectExportProviderHelper.GetExportsRecursive(
//                 _cache,
//                 applicationHostContext.LibraryManager,
//                 applicationHostContext.LibraryExportProvider,
//                 new LibraryKey {
//                     Configuration = configuration,
//                     TargetFramework = frameworkName,
//                     Name = project.Name
//                 },
//                 library => library.Type != "Project");

//            foreach (var reference in exportWithoutProjects.MetadataReferences) {
//                var fileReference = reference as IMetadataFileReference;
//                if (fileReference != null) {
//                    info.References.Add(fileReference.Path);
//                }

//                var embedded = reference as IMetadataEmbeddedReference;
//                if (embedded != null) {
//                    info.RawReferences[embedded.Name] = embedded.Contents;
//                }
//            }

//            return info;
//            //});
//        }

//        private ApplicationHostContext GetApplicationHostContext(Project project, string configuration, FrameworkName frameworkName, bool useRuntimeLoadContextFactory = true) {
//            var cacheKey = Tuple.Create("ApplicationContext", project.Name, configuration, frameworkName);

//            IAssemblyLoadContextFactory loadContextFactory = null;

//            if (useRuntimeLoadContextFactory) {
//                var runtimeApplicationContext = GetApplicationHostContext(project,
//                                                                          _appEnv.Configuration,
//                                                                          _appEnv.RuntimeFramework,
//                                                                          useRuntimeLoadContextFactory: false);

//                loadContextFactory = runtimeApplicationContext.AssemblyLoadContextFactory;
//            }

//            return _cache.Get<ApplicationHostContext>(cacheKey, ctx => {
//                var applicationHostContext = new ApplicationHostContext(_hostServices,
//                                                                        project.ProjectDirectory,
//                                                                        packagesDirectory: null,
//                                                                        configuration: configuration,
//                                                                        targetFramework: frameworkName,
//                                                                        cache: _cache,
//                                                                        cacheContextAccessor: _cacheContextAccessor,
//                                                                        namedCacheDependencyProvider: _namedDependencyProvider,
//                                                                        loadContextFactory: loadContextFactory);

//                applicationHostContext.DependencyWalker.Walk(project.Name, project.Version, frameworkName);

//                // Watch all projects for project.json changes
//                foreach (var library in applicationHostContext.DependencyWalker.Libraries) {
//                    if (string.Equals(library.Type, "Project")) {
//                        ctx.Monitor(new FileWriteTimeCacheDependency(library.Path));
//                    }
//                }

//                // Add a cache dependency on restore complete to reevaluate dependencies
//                ctx.Monitor(_namedDependencyProvider.GetNamedDependency(project.Name + "_BuildOutputs"));

//                return applicationHostContext;
//            });
//        }

//        private static DependencyDescription CreateDependencyDescription(LibraryDescription library) {
//            return new DependencyDescription {
//                Name = library.Identity.Name,
//                Version = library.Identity.Version == null ? null : library.Identity.Version.ToString(),
//                Type = library.Type ?? "Unresolved",
//                Path = library.Path,
//                Dependencies = library.Dependencies.Select(lib => new DependencyItem {
//                    Name = lib.Name,
//                    Version = lib.Version == null ? null : lib.Version.ToString()
//                })
//            };
//        }

//        private class State {
//            public string Name { get; set; }

//            public IList<string> ProjectSearchPaths { get; set; }

//            public string GlobalJsonPath { get; set; }

//            public IList<string> Configurations { get; set; }

//            public IList<FrameworkData> Frameworks { get; set; }

//            public IDictionary<string, string> Commands { get; set; }

//            public IList<ProjectInfo> Projects { get; set; }
//        }

//        // Represents a project that should be used for intellisense
//        private class ProjectInfo {
//            public string Path { get; set; }

//            public string Configuration { get; set; }

//            public FrameworkName FrameworkName { get; set; }

//            public FrameworkData TargetFramework { get; set; }

//            public CompilationSettings CompilationSettings { get; set; }

//            public IList<string> SourceFiles { get; set; }

//            public DependencyInfo DependencyInfo { get; set; }
//        }

//        private class DependencyInfo {
//            public ApplicationHostContext HostContext { get; set; }

//            public IDictionary<string, byte[]> RawReferences { get; set; }

//            public IDictionary<string, DependencyDescription> Dependencies { get; set; }

//            public IList<string> References { get; set; }

//            public IList<ProjectReference> ProjectReferences { get; set; }
//        }

//        public class FrameworkData {
//            public string FrameworkName { get; set; }
//            public string FriendlyName { get; set; }
//            public string ShortName { get; set; }
//            public string RedistListPath { get; set; }

//            public override bool Equals(object obj) {
//                var other = obj as FrameworkData;

//                return other != null &&
//                       string.Equals(FrameworkName, other.FrameworkName);
//            }

//            public override int GetHashCode() {
//                // These objects are currently POCOs and we're overriding equals
//                // so that things like Enumerable.SequenceEqual just work.
//                return base.GetHashCode();
//            }
//        }

//        public class DependencyDescription {
//            public string Name { get; set; }

//            public string Version { get; set; }

//            public string Path { get; set; }

//            public string Type { get; set; }

//            public IEnumerable<DependencyItem> Dependencies { get; set; }

//            public override bool Equals(object obj) {
//                var other = obj as DependencyDescription;

//                return other != null &&
//                       string.Equals(Name, other.Name) &&
//                       object.Equals(Version, other.Version) &&
//                       string.Equals(Path, other.Path) &&
//                       string.Equals(Type, other.Type) &&
//                       Enumerable.SequenceEqual(Dependencies, other.Dependencies);
//            }

//            public override int GetHashCode() {
//                // These objects are currently POCOs and we're overriding equals
//                // so that things like Enumerable.SequenceEqual just work.
//                return base.GetHashCode();
//            }
//        }

//        public class ProjectReference {
//            public FrameworkData Framework { get; set; }
//            public string Path { get; set; }

//            public override bool Equals(object obj) {
//                var other = obj as ProjectReference;
//                return other != null &&
//                       string.Equals(Framework, other.Framework) &&
//                       object.Equals(Path, other.Path);
//            }

//            public override int GetHashCode() {
//                // These objects are currently POCOs and we're overriding equals
//                // so that things like Enumerable.SequenceEqual just work.
//                return base.GetHashCode();
//            }
//        }

//        public class DependencyItem {
//            public string Name { get; set; }

//            public string Version { get; set; }

//            public override bool Equals(object obj) {
//                var other = obj as DependencyItem;
//                return other != null &&
//                       string.Equals(Name, other.Name) &&
//                       object.Equals(Version, other.Version);
//            }

//            public override int GetHashCode() {
//                // These objects are currently POCOs and we're overriding equals
//                // so that things like Enumerable.SequenceEqual just work.
//                return base.GetHashCode();
//            }
//        }

//        private class LibraryKey : ILibraryKey {
//            public string Name { get; set; }
//            public FrameworkName TargetFramework { get; set; }
//            public string Configuration { get; set; }
//            public string Aspect { get; set; }
//        }
//    }
//}