using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Extensions.Manifests;
using OrchardCore.Environment.Extensions.Utility;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Modules;

// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ParameterTypeCanBeEnumerable.Local
// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable LoopCanBeConvertedToQuery

namespace OrchardCore.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private IFeatureInfo _applicationFeature;

        private readonly IApplicationContext _applicationContext;

        private readonly IExtensionDependencyStrategy[] _extensionDependencyStrategies;
        private readonly IExtensionPriorityStrategy[] _extensionPriorityStrategies;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IServiceCollection _applicationServices;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFeaturesProvider _featuresProvider;

        private FrozenDictionary<string, ExtensionEntry> _extensions;
        private List<IExtensionInfo> _extensionsInfos;
        private Dictionary<string, FeatureEntry> _features;
        private IFeatureInfo[] _featureInfos;

        private readonly ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies = new();
        private readonly ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _dependentFeatures = new();

        private bool _isInitialized;
        private readonly SemaphoreSlim _semaphore = new(1);

        public ExtensionManager(
            IApplicationContext applicationContext,
            IEnumerable<IExtensionDependencyStrategy> extensionDependencyStrategies,
            IEnumerable<IExtensionPriorityStrategy> extensionPriorityStrategies,
            ITypeFeatureProvider typeFeatureProvider,
            IHostEnvironment hostingEnvironment,
            IServiceCollection applicationServices,
            IServiceProvider serviceProvider,
            IFeaturesProvider featuresProvider,
            ILogger<ExtensionManager> logger)
        {
            _applicationContext = applicationContext;
            _extensionDependencyStrategies = extensionDependencyStrategies as IExtensionDependencyStrategy[] ?? extensionDependencyStrategies.ToArray();
            _extensionPriorityStrategies = extensionPriorityStrategies as IExtensionPriorityStrategy[] ?? extensionPriorityStrategies.ToArray();
            _typeFeatureProvider = typeFeatureProvider;
            _hostingEnvironment = hostingEnvironment;
            _applicationServices = applicationServices;
            _serviceProvider = serviceProvider;
            _featuresProvider = featuresProvider;
            L = logger;
        }

        public ILogger L { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            EnsureInitialized();

            if (!string.IsNullOrEmpty(extensionId) && _extensions.TryGetValue(extensionId, out var extension))
            {
                return extension.ExtensionInfo;
            }

            return new NotFoundExtensionInfo(extensionId);
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            EnsureInitialized();

            return _extensionsInfos;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        {
            EnsureInitialized();

            var allDependencyIds = new HashSet<string>(featureIdsToLoad
                .SelectMany(featureId => GetFeatureDependencies(featureId))
                .Select(x => x.Id));

            foreach (var featureInfo in _featureInfos)
            {
                if (allDependencyIds.Contains(featureInfo.Id))
                {
                    yield return featureInfo;
                }
            }
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            EnsureInitialized();

            _extensions.TryGetValue(extensionInfo.Id, out var extension);

            return Task.FromResult(extension);
        }

        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync()
        {
            await EnsureInitializedAsync();
            return _features.Values;
        }

        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(string[] featureIdsToLoad)
        {
            await EnsureInitializedAsync();

            var features = new HashSet<string>(GetFeatures(featureIdsToLoad).Select(f => f.Id));

            var loadedFeatures = _features.Values
                .Where(f => features.Contains(f.FeatureInfo.Id));

            return loadedFeatures;
        }

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            EnsureInitialized();

            return _featureDependencies.GetOrAdd(featureId, (key) => new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                if (!_features.TryGetValue(key, out var entry))
                {
                    return [];
                }

                var feature = entry.FeatureInfo;

                return GetFeatureDependencies(feature, _featureInfos);
            })).Value;
        }

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            EnsureInitialized();

            return _dependentFeatures.GetOrAdd(featureId, (key) => new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                if (!_features.TryGetValue(key, out var entry))
                {
                    return [];
                }

                var feature = entry.FeatureInfo;

                return GetDependentFeatures(feature, _featureInfos);
            })).Value;
        }

        private IEnumerable<IFeatureInfo> GetFeatureDependencies(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencyIds = new HashSet<string> { feature.Id };
            var stack = new Stack<List<IFeatureInfo>>();

            stack.Push(GetFeatureDependenciesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next)
                {
                    if (!dependencyIds.Contains(dependency.Id))
                    {
                        dependencyIds.Add(dependency.Id);
                        stack.Push(GetFeatureDependenciesFunc(dependency, features));
                    }
                }
            }

            // Preserve the underlying order of feature infos.
            foreach (var featureInfo in _featureInfos)
            {
                if (dependencyIds.Contains(featureInfo.Id))
                {
                    yield return featureInfo;
                }
            }
        }

        private IEnumerable<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencyIds = new HashSet<string> { feature.Id };
            var stack = new Stack<List<IFeatureInfo>>();

            stack.Push(GetDependentFeaturesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next)
                {
                    if (!dependencyIds.Contains(dependency.Id))
                    {
                        dependencyIds.Add(dependency.Id);
                        stack.Push(GetDependentFeaturesFunc(dependency, features));
                    }
                }
            }

            // Preserve the underlying order of feature infos.
            foreach (var featureInfo in _featureInfos)
            {
                if (dependencyIds.Contains(featureInfo.Id))
                {
                    yield return featureInfo;
                }
            }
        }

        public IEnumerable<IFeatureInfo> GetFeatures()
        {
            EnsureInitialized();

            return _featureInfos;
        }

        private static List<IFeatureInfo> GetDependentFeaturesFunc(IFeatureInfo currentFeature, IFeatureInfo[] features)
        {
            var list = new List<IFeatureInfo>();
            foreach (var f in features)
            {
                foreach (var dependencyId in f.Dependencies)
                {
                    if (dependencyId == currentFeature.Id)
                    {
                        list.Add(f);
                        break;
                    }
                }
            }

            return list;
        }

        private static List<IFeatureInfo> GetFeatureDependenciesFunc(IFeatureInfo currentFeature, IFeatureInfo[] features)
        {
            var list = new List<IFeatureInfo>();
            foreach (var f in features)
            {
                foreach (var dependencyId in currentFeature.Dependencies)
                {
                    if (dependencyId == f.Id)
                    {
                        list.Add(f);
                        break;
                    }
                }
            }

            return list;
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionId)
        {
            var attribute = type.GetCustomAttributes<FeatureAttribute>(false).FirstOrDefault();

            return attribute?.FeatureName ?? extensionId;
        }

        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            EnsureInitializedAsync().GetAwaiter().GetResult();
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (_isInitialized)
                {
                    return;
                }

                var modules = _applicationContext.Application.Modules;
                var loadedExtensions = new ConcurrentDictionary<string, ExtensionEntry>();

                // Load all extensions in parallel
                await modules.ForEachAsync((module) =>
                {
                    if (!module.ModuleInfo.Exists)
                    {
                        return Task.CompletedTask;
                    }

                    var manifestInfo = new ManifestInfo(module.ModuleInfo);
                    var extensionInfo = new ExtensionInfo(module.SubPath, manifestInfo, (mi, ei) =>
                    {
                        return _featuresProvider.GetFeatures(ei, mi);
                    });

                    var entry = new ExtensionEntry
                    {
                        ExtensionInfo = extensionInfo,
                        Assembly = module.Assembly,
                        ExportedTypes = module.Assembly.ExportedTypes
                    };

                    loadedExtensions.TryAdd(module.Name, entry);

                    return Task.CompletedTask;
                });

                // Get all valid types from any extension

                var allComponentTypes = loadedExtensions.SelectMany(extension =>
                    extension.Value.ExportedTypes.Where(IsComponentType)).ToArray();

                var allTypesByExtension = loadedExtensions.SelectMany(extension =>
                    extension.Value.ExportedTypes.Where(IsComponentType)
                    .Select(type => new
                    {
                        ExtensionEntry = extension.Value,
                        Type = type
                    })).ToArray();

                var typesByFeature = allTypesByExtension
                    .GroupBy(typeByExtension => GetSourceFeatureNameForType(
                        typeByExtension.Type,
                        typeByExtension.ExtensionEntry.ExtensionInfo.Id))
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(typesByExtension => typesByExtension.Type).ToArray());

                var startupByFeatureTypes = typesByFeature.ToDictionary(x => x.Key, x => x.Value.Where(IsComponentType).Where(x => x.Name == "Startup" || typeof(IStartup).IsAssignableFrom(x)));

                var tenantServiceCollection = _serviceProvider.CreateChildContainer(_applicationServices);

                var settings = new ShellSettings();

                tenantServiceCollection.AddSingleton(settings);
                tenantServiceCollection.AddSingleton(sp =>
                {
                    // Resolve it lazily as it's constructed lazily
                    var shellSettings = sp.GetRequiredService<ShellSettings>();
                    return shellSettings.ShellConfiguration;
                });

                // To not trigger features loading before it is normally done by 'ShellHost',
                // init here the application feature in place of doing it in the constructor.
                _applicationFeature = loadedExtensions.SelectMany(x => x.Value.ExtensionInfo.Features)
                    .FirstOrDefault(x => x.Id == _hostingEnvironment.ApplicationName);

                var applicationTypes = allTypesByExtension.Where(x => x.ExtensionEntry.ExtensionInfo.Id == _applicationFeature.Id)
                    .Select(x => x.Type)
                    .ToArray();

                foreach (var startup in allComponentTypes.Where(t => typeof(IStartup).IsAssignableFrom(t)))
                {
                    tenantServiceCollection.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IStartup), startup));
                }

                foreach (var rawStartup in allComponentTypes.Where(t => t.Name == "Startup"))
                {
                    // Startup classes inheriting from IStartup are already treated
                    if (typeof(IStartup).IsAssignableFrom(rawStartup))
                    {
                        continue;
                    }

                    // Ignore Startup class from main application
                    if (applicationTypes.Contains(rawStartup))
                    {
                        continue;
                    }

                    // Create a wrapper around this method
                    var configureServicesMethod = rawStartup.GetMethod(
                        nameof(IStartup.ConfigureServices),
                        BindingFlags.Public | BindingFlags.Instance,
                        null,
                        CallingConventions.Any,
                        [typeof(IServiceCollection)],
                        null);

                    var configureMethod = rawStartup.GetMethod(
                        nameof(IStartup.Configure),
                        BindingFlags.Public | BindingFlags.Instance);

                    var orderProperty = rawStartup.GetProperty(
                        nameof(IStartup.Order),
                        BindingFlags.Public | BindingFlags.Instance);

                    var configureOrderProperty = rawStartup.GetProperty(
                        nameof(IStartup.ConfigureOrder),
                        BindingFlags.Public | BindingFlags.Instance);

                    // Add the startup class to the DI so we can instantiate it with
                    // valid ctor arguments
                    tenantServiceCollection.AddSingleton(rawStartup);

                    tenantServiceCollection.AddSingleton<IStartup>(sp =>
                    {
                        var startupInstance = sp.GetService(rawStartup);
                        return new StartupBaseMock(startupInstance, configureServicesMethod, configureMethod, orderProperty, configureOrderProperty);
                    });
                }

                var shellServiceProvider = tenantServiceCollection.BuildServiceProvider(true);

                var startups = shellServiceProvider.GetServices<IStartup>();

                var loadedFeatures = new Dictionary<string, FeatureEntry>();

                var typesByExtension = new Dictionary<string, Type[]>();

                foreach (var loadedExtension in loadedExtensions)
                {
                    foreach (var feature in loadedExtension.Value.ExtensionInfo.Features)
                    {
                        if (startupByFeatureTypes.TryGetValue(feature.Id, out var loadedExtensionStartupTypes))
                        {
                            var resolvedStartups = startups.Where(x => loadedExtensionStartupTypes.Contains(x.GetType()));

                            foreach (var resolvedStartup in resolvedStartups)
                            {
                                var serviceCollection = new ServiceCollection();

                                resolvedStartup.ConfigureServices(serviceCollection);

                                var attribute = resolvedStartup.GetType().GetCustomAttributes<FeatureAttribute>(false).FirstOrDefault();

                                var exportedTypes = serviceCollection.Select(x => x.GetImplementationType())
                                    .Where(x => x != null && x.IsClass && !x.IsAbstract && x.IsPublic)
                                    .ToArray();

                                foreach (var exportedType in exportedTypes)
                                {
                                    _typeFeatureProvider.TryAdd(exportedType, feature);
                                }

                                var featureId = attribute?.FeatureName ?? loadedExtension.Key;

                                if (loadedFeatures.TryGetValue(featureId, out var entry))
                                {
                                    loadedFeatures[featureId] = new FeatureEntry(feature, entry.ExportedTypes.Concat(exportedTypes));
                                }
                                else
                                {
                                    loadedFeatures[featureId] = new FeatureEntry(feature, exportedTypes);
                                }
                            }

                            continue;
                        }

                        // Features can have no types
                        if (typesByFeature.TryGetValue(feature.Id, out var featureTypes))
                        {
                            foreach (var type in featureTypes)
                            {
                                _typeFeatureProvider.TryAdd(type, feature);
                            }
                        }

                        loadedFeatures.Add(feature.Id, new FeatureEntry(feature, featureTypes ?? []));
                    }
                }

                await shellServiceProvider.DisposeAsync();

                // Feature infos and entries are ordered by priority and dependencies.
                _featureInfos = Order(loadedFeatures.Values.Select(f => f.FeatureInfo));
                _features = _featureInfos.ToDictionary(f => f.Id, f => loadedFeatures[f.Id]);

                // Extensions are also ordered according to the weight of their first features.
                _extensionsInfos = _featureInfos
                    .Where(f => f.Id == f.Extension.Features.First().Id)
                    .Select(f => f.Extension)
                    .ToList();

                _extensions = _extensionsInfos.ToFrozenDictionary(e => e.Id, e => loadedExtensions[e.Id]);

                _isInitialized = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static bool IsComponentType(Type type)
        {
            return type.IsClass && !type.IsAbstract && type.IsPublic;
        }

        private IFeatureInfo[] Order(IEnumerable<IFeatureInfo> featuresToOrder)
        {
            return featuresToOrder
                .OrderBy(x => x.Id)
                .OrderByDependenciesAndPriorities(HasDependency, GetPriority)
                .ToArray();
        }

        private bool HasDependency(IFeatureInfo f1, IFeatureInfo f2)
        {
            foreach (var s in _extensionDependencyStrategies)
            {
                if (s.HasDependency(f1, f2))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetPriority(IFeatureInfo feature)
        {
            var sum = 0;
            foreach (var strategy in _extensionPriorityStrategies)
            {
                sum += strategy.GetPriority(feature);
            }

            return sum;
        }
    }
}
