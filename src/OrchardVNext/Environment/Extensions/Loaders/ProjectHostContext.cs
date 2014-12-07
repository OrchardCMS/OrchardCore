using System;
using System.Runtime.Versioning;
using Microsoft.Framework.Runtime;
using Microsoft.Framework.Runtime.FileSystem;
using Microsoft.Framework.Runtime.Loader;
using Microsoft.Framework.DependencyInjection;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection.ServiceLookup;
using System.Linq;
using Microsoft.Framework.Runtime.Roslyn.Services;
using Microsoft.Framework.Runtime.Roslyn;
using System.IO;

namespace OrchardVNext.Environment.Extensions.Loaders {
    public class ProjectHostContext {
        private readonly ServiceProvider _serviceProvider;

        public ProjectHostContext(IServiceProvider serviceProvider,
                                      string projectDirectory,
                                      string packagesDirectory,
                                      string[] sourcePaths,
                                      string configuration,
                                      FrameworkName targetFramework,
                                      ICache cache,
                                      ICacheContextAccessor cacheContextAccessor,
                                      INamedCacheDependencyProvider namedCacheDependencyProvider,
                                      IAssemblyLoadContextFactory loadContextFactory = null) {
            ProjectDirectory = projectDirectory;
            Configuration = configuration;
            RootDirectory = Microsoft.Framework.Runtime.ProjectResolver.ResolveRootDirectory(ProjectDirectory);
            ProjectResolver = new ProjectResolver(ProjectDirectory, RootDirectory, sourcePaths);
            FrameworkReferenceResolver = new FrameworkReferenceResolver();
            _serviceProvider = new ServiceProvider(serviceProvider);

            PackagesDirectory = packagesDirectory ?? NuGetDependencyResolver.ResolveRepositoryPath(RootDirectory);

            var referenceAssemblyDependencyResolver = new ReferenceAssemblyDependencyResolver(FrameworkReferenceResolver);
            NuGetDependencyProvider = new NuGetDependencyResolver(PackagesDirectory, RootDirectory);
            var gacDependencyResolver = new GacDependencyResolver();
            ProjectDepencyProvider = new ProjectReferenceDependencyProvider(ProjectResolver);
            var unresolvedDependencyProvider = new UnresolvedDependencyProvider();

            DependencyWalker = new DependencyWalker(new IDependencyProvider[] {
                ProjectDepencyProvider,
                NuGetDependencyProvider,
                referenceAssemblyDependencyResolver,
                gacDependencyResolver,
                unresolvedDependencyProvider
            });

            LibraryExportProvider = new CompositeLibraryExportProvider(new ILibraryExportProvider[] {
                new ProjectLibraryExportProvider(ProjectResolver, ServiceProvider),
                referenceAssemblyDependencyResolver,
                gacDependencyResolver,
                NuGetDependencyProvider
            });

            LibraryManager = new LibraryManager(targetFramework, configuration, DependencyWalker,
                LibraryExportProvider, cache);

            AssemblyLoadContextFactory = loadContextFactory ?? new AssemblyLoadContextFactory(ServiceProvider);

            var sourceCodeService = new SourceTextService(cache);

            var provider = new RoslynProjectReferenceProvider(cache, cacheContextAccessor, namedCacheDependencyProvider, loadContextFactory, _serviceProvider.GetService<IFileWatcher>(), _serviceProvider);

            // Default services
            _serviceProvider.Add(typeof(IApplicationEnvironment), new ApplicationEnvironment(Project, targetFramework, configuration));
            _serviceProvider.Add(typeof(ILibraryExportProvider), LibraryExportProvider, includeInManifest: false);
            _serviceProvider.Add(typeof(IProjectResolver), ProjectResolver);
            _serviceProvider.Add(typeof(IFileWatcher), NoopWatcher.Instance);

            _serviceProvider.Add(typeof(NuGetDependencyResolver), NuGetDependencyProvider, includeInManifest: false);
            _serviceProvider.Add(typeof(ProjectReferenceDependencyProvider), ProjectDepencyProvider, includeInManifest: false);
            _serviceProvider.Add(typeof(ILibraryManager), LibraryManager);
            _serviceProvider.Add(typeof(ICache), cache);
            _serviceProvider.Add(typeof(ICacheContextAccessor), cacheContextAccessor);
            _serviceProvider.Add(typeof(INamedCacheDependencyProvider), namedCacheDependencyProvider, includeInManifest: false);
            _serviceProvider.Add(typeof(IAssemblyLoadContextFactory), AssemblyLoadContextFactory);
            
            _serviceProvider.Add(typeof(IProjectReferenceProvider), provider);
            _serviceProvider.Add(typeof(ISourceTextService), sourceCodeService);
        }

        public void AddService(Type type, object instance, bool includeInManifest) {
            _serviceProvider.Add(type, instance, includeInManifest);
        }

        public void AddService(Type type, object instance) {
            _serviceProvider.Add(type, instance);
        }

        public T CreateInstance<T>() {
            return ActivatorUtilities.CreateInstance<T>(_serviceProvider);
        }

        public IServiceProvider ServiceProvider {
            get {
                return _serviceProvider;
            }
        }

        public Project Project {
            get {
                Project project;
                if (Project.TryGetProject(ProjectDirectory, out project)) {
                    return project;
                }
                return null;
            }
        }

        public IAssemblyLoadContextFactory AssemblyLoadContextFactory { get; private set; }

        public NuGetDependencyResolver NuGetDependencyProvider { get; private set; }

        public ProjectReferenceDependencyProvider ProjectDepencyProvider { get; private set; }

        public IProjectResolver ProjectResolver { get; private set; }
        public ILibraryExportProvider LibraryExportProvider { get; private set; }
        public ILibraryManager LibraryManager { get; private set; }
        public DependencyWalker DependencyWalker { get; private set; }
        public FrameworkReferenceResolver FrameworkReferenceResolver { get; private set; }

        public string Configuration { get; private set; }
        public string RootDirectory { get; private set; }
        public string ProjectDirectory { get; private set; }
        public string PackagesDirectory { get; private set; }
    }

    internal class ServiceProvider : IServiceProvider {
        private readonly Dictionary<Type, ServiceEntry> _entries = new Dictionary<Type, ServiceEntry>();
        private readonly IServiceProvider _fallbackServiceProvider;

        public ServiceProvider() {
            Add(typeof(IServiceProvider), this, includeInManifest: false);
            Add(typeof(IServiceManifest), new ServiceManifest(this), includeInManifest: false);
        }

        public ServiceProvider(IServiceProvider fallbackServiceProvider)
            : this() {
            _fallbackServiceProvider = fallbackServiceProvider;
        }

        public void Add(Type type, object instance) {
            Add(type, instance, includeInManifest: true);
        }

        public void Add(Type type, object instance, bool includeInManifest) {
            _entries[type] = new ServiceEntry {
                Instance = instance,
                IncludeInManifest = includeInManifest
            };
        }

        public object GetService(Type serviceType) {
            ServiceEntry entry;
            if (_entries.TryGetValue(serviceType, out entry)) {
                return entry.Instance;
            }

            Array serviceArray = GetServiceArrayOrNull(serviceType);

            if (serviceArray != null && serviceArray.Length != 0) {
                return serviceArray;
            }

            if (_fallbackServiceProvider != null) {
                return _fallbackServiceProvider.GetService(serviceType);
            }

            return serviceArray;
        }

        private Array GetServiceArrayOrNull(Type serviceType) {
            var typeInfo = serviceType.GetTypeInfo();

            if (typeInfo.IsGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                var itemType = typeInfo.GenericTypeArguments[0];

                ServiceEntry entry;
                if (_entries.TryGetValue(itemType, out entry)) {
                    var serviceArray = Array.CreateInstance(itemType, 1);
                    serviceArray.SetValue(entry.Instance, 0);
                    return serviceArray;
                }
                else {
                    return Array.CreateInstance(itemType, 0);
                }
            }

            return null;
        }

        private IEnumerable<Type> GetManifestServices() {
            var services = _entries.Where(p => p.Value.IncludeInManifest)
                                   .Select(p => p.Key);

            var fallbackManifest = _fallbackServiceProvider?.GetService(typeof(IServiceManifest)) as IServiceManifest;

            if (fallbackManifest != null) {
                return fallbackManifest.Services.Concat(services);
            }

            return services;
        }

        private class ServiceEntry {
            public object Instance { get; set; }
            public bool IncludeInManifest { get; set; }
        }

        private class ServiceManifest : IServiceManifest {
            private readonly ServiceProvider _serviceProvider;

            public ServiceManifest(ServiceProvider serviceProvider) {
                _serviceProvider = serviceProvider;
            }

            public IEnumerable<Type> Services {
                get {
                    return _serviceProvider.GetManifestServices().Distinct();
                }
            }
        }
    }

    public class ProjectResolver : IProjectResolver {
        private readonly IList<string> _searchPaths;

        public ProjectResolver(
            string projectPath, 
            string rootPath, 
            string[] sourcePaths) {
            // Go hunting for projects.
            _searchPaths = ResolveSearchPaths(projectPath, rootPath, sourcePaths).ToList();
        }

        public IEnumerable<string> SearchPaths {
            get {
                return _searchPaths;
            }
        }

        public bool TryResolveProject(string name, out Project project) {
            project = _searchPaths.Select(path => Path.Combine(path, name))
                                  .Select(path => GetProject(path))
                                  .FirstOrDefault(p => p != null);

            return project != null;
        }

        private Project GetProject(string path) {
            Project project;
            if (Project.TryGetProject(path, out project)) {

                LanguageServices obj = new LanguageServices("C#", new TypeInformation("Microsoft.Framework.Runtime.Roslyn", "Microsoft.Framework.Runtime.Roslyn.RoslynProjectReferenceProvider"));

                var prop = project.GetType().GetProperty("LanguageServices", BindingFlags.Public | BindingFlags.Instance);
                if (null != prop && prop.CanWrite) {
                    prop.SetValue(project, obj, null);
                }
            }
            return project;
        }

        private IEnumerable<string> ResolveSearchPaths(string projectPath, string rootPath, string[] sourcePaths) {
            var paths = new List<string>
            {
                Path.GetDirectoryName(projectPath)
            };

            GlobalSettings global;

            if (GlobalSettings.TryGetGlobalSettings(rootPath, out global)) {
                sourcePaths = sourcePaths.Concat(global.SourcePaths).ToArray();
            }

            foreach (var sourcePath in sourcePaths) {
                paths.Add(Path.Combine(rootPath, sourcePath));
            }

            return paths.Distinct();
        }

        public static string ResolveRootDirectory(string projectPath) {
            var di = new DirectoryInfo(projectPath);

            while (di.Parent != null) {
                if (di.EnumerateFiles(GlobalSettings.GlobalFileName).Any() ||
                    di.EnumerateFiles("*.sln").Any()) {
                    return di.FullName;
                }

                di = di.Parent;
            }

            // If we don't find any files then make the project folder the root
            return projectPath;
        }
    }
}