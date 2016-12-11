using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Environment.Extensions.Utility;

namespace Orchard.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly ExtensionOptions _extensionOptions;
        private readonly IExtensionProvider _extensionProvider;
        private readonly IExtensionLoader _extensionLoader;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private readonly ConcurrentDictionary<string, Lazy<Task<ExtensionEntry>>> _extensions
            = new ConcurrentDictionary<string, Lazy<Task<ExtensionEntry>>>();

        private readonly ConcurrentDictionary<string, Lazy<Task<FeatureEntry>>> _features
            = new ConcurrentDictionary<string, Lazy<Task<FeatureEntry>>>();

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _featureDependencies
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

        private ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>> _dependentFeatures
            = new ConcurrentDictionary<string, Lazy<IEnumerable<IFeatureInfo>>>();

        private IDictionary<string, IExtensionInfo> _extensionsById;

        private ConcurrentBag<IFeatureInfo> _allOrderedFeatureInfos;
        private ConcurrentBag<IFeatureInfo> _allUnorderedFeatureInfos;

        private static Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> GetDependantFeaturesFunc =
            new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                (currentFeature, fs) => fs
                    .Where(f =>
                            f.Dependencies.Any(dep => dep == currentFeature.Id)
                           ).OrderBy(x => x.Id).ToArray());

        private static Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> GetFeatureDependenciesFunc =
            new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                (currentFeature, fs) => fs
                    .Where(f =>
                            currentFeature.Dependencies.Any(dep => dep == f.Id)
                           ).OrderBy(x => x.Id).ToArray());

        public ExtensionManager(
            IOptions<ExtensionOptions> optionsAccessor,
            IEnumerable<IExtensionProvider> extensionProviders,
            IEnumerable<IExtensionLoader> extensionLoaders,
            IHostingEnvironment hostingEnvironment,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<ExtensionManager> logger,
            IStringLocalizer<ExtensionManager> localizer)
        {
            _extensionOptions = optionsAccessor.Value;
            _extensionProvider = new CompositeExtensionProvider(extensionProviders);
            _extensionLoader = new CompositeExtensionLoader(extensionLoaders);
            _hostingEnvironment = hostingEnvironment;
            _typeFeatureProvider = typeFeatureProvider;
            L = logger;
            T = localizer;
        }
        public ILogger L { get; set; }
        public IStringLocalizer T { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            GetExtensions(); // initialize

            return _extensionsById[extensionId];
        }

        public IEnumerable<IExtensionInfo> GetExtensions()
        {
            if (_extensionsById == null)
            {
                var extensionsById = new Dictionary<string, IExtensionInfo>();

                foreach (var searchPath in _extensionOptions.SearchPaths)
                {
                    foreach (var subDirectory in _hostingEnvironment
                        .ContentRootFileProvider
                        .GetDirectoryContents(searchPath)
                        .Where(x => x.IsDirectory))
                    {
                        var extensionId = subDirectory.Name;

                        if (!extensionsById.ContainsKey(extensionId))
                        {
                            var subPath = Path.Combine(searchPath, extensionId);

                            var extensionInfo =
                                _extensionProvider.GetExtensionInfo(subPath);

                            if (extensionInfo.ExtensionFileInfo.Exists)
                            {
                                extensionsById.Add(extensionId, extensionInfo);
                            }
                        }
                    }
                }

                _extensionsById = extensionsById;
            }

            return _extensionsById.Values;
        }

        public IEnumerable<IFeatureInfo> GetFeatureDependencies(string featureId)
        {
            return _featureDependencies.GetOrAdd(featureId,
                new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                var unorderedFeatures = GetAllUnorderedFeatures().ToArray();

                var feature = unorderedFeatures.FirstOrDefault(x => x.Id == featureId);
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                var dependencies = new HashSet<IFeatureInfo>() { feature };
                var stack = new Stack<IFeatureInfo[]>();

                stack.Push(GetFeatureDependenciesFunc(feature, unorderedFeatures));

                while (stack.Count > 0)
                {
                    var next = stack.Pop();
                    foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                    {
                        dependencies.Add(dependency);
                        stack.Push(GetFeatureDependenciesFunc(dependency, unorderedFeatures));
                    }
                }

                return dependencies.Reverse();
            })).Value;
        }

        public IEnumerable<IFeatureInfo> GetDependentFeatures(string featureId)
        {
            return _dependentFeatures.GetOrAdd(featureId,
                 new Lazy<IEnumerable<IFeatureInfo>>(() =>
            {
                var unorderedFeatures = GetAllUnorderedFeatures();

                var feature = unorderedFeatures.FirstOrDefault(x => x.Id == featureId);
                if (feature == null)
                {
                    return Enumerable.Empty<IFeatureInfo>();
                }

                return
                    GetDependentFeatures(feature, unorderedFeatures.ToArray());

            })).Value;
        }

        private IEnumerable<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features)
        {
            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(GetDependantFeaturesFunc(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(GetDependantFeaturesFunc(dependency, features));
                }
            }

            return dependencies;
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.
            return _extensions.GetOrAdd(extensionInfo.Id,
                new Lazy<Task<ExtensionEntry>>(() =>
            {
                var extension = _extensionLoader.Load(extensionInfo);

                if (extension.IsError && L.IsEnabled(LogLevel.Warning))
                {

                    L.LogError("No suitable loader found for extension \"{0}\"", extensionInfo.Id);
                }

                return Task.FromResult(extension);
            })).Value;
        }

        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync()
        {
            // get loaded feature information
            var loadedFeatures = await Task.WhenAll(GetFeatures()
                .Select(feature => LoadFeatureAsync(feature))
                .ToArray());

            return loadedFeatures.AsEnumerable();
        }

        public IEnumerable<IFeatureInfo> GetFeatures()
        {
            if (_allOrderedFeatureInfos == null)
            {
                var allUnorderedFeatures = GetAllUnorderedFeatures();

                var orderedFeatureDescriptors = allUnorderedFeatures
                    .OrderByDependenciesAndPriorities(
                        (fiObv, fiSub) => GetDependentFeatures(fiObv.Id).Contains(fiSub),
                        (fi) => fi.Priority);

                _allOrderedFeatureInfos = new ConcurrentBag<IFeatureInfo>(orderedFeatureDescriptors);
            }

            return _allOrderedFeatureInfos;
        }

        public IEnumerable<IFeatureInfo> GetFeatures(string[] featureIdsToLoad)
        {
            var allUnorderedFeaturesToLoadIncludingDependencies = featureIdsToLoad
                .SelectMany(featureId => GetFeatureDependencies(featureId))
                .Distinct()
                .ToArray();

            return allUnorderedFeaturesToLoadIncludingDependencies
                .OrderByDependenciesAndPriorities(
                    (fiObv, fiSub) => GetDependentFeatures(fiObv.Id).Contains(fiSub),
                    (fi) => fi.Priority)
                .Reverse();
        }
        
        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(string[] featureIdsToLoad)
        {
            var features = GetFeatures(featureIdsToLoad);

            // get loaded feature information
            var loadedFeatures = await Task.WhenAll(features
                .Select(feature => LoadFeatureAsync(feature))
                .ToArray());

            return loadedFeatures.AsEnumerable();
        }

        private Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
        {
            return _features.GetOrAdd(feature.Id,
                new Lazy<Task<FeatureEntry>>(async () =>
                {
                    var loadedExtension = await LoadExtensionAsync(feature.Extension);

                if (loadedExtension.IsError)
                {
                    return new NonCompiledFeatureEntry(feature);
                }

                var extensionTypes = loadedExtension
                    .ExportedTypes
                    .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);

                var featureTypes = new List<Type>();

                foreach (var type in extensionTypes)
                {
                    string sourceFeature = GetSourceFeatureNameForType(type, feature.Id);
                    if (sourceFeature == feature.Id)
                    {
                        featureTypes.Add(type);
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }

                return new CompiledFeatureEntry(feature, featureTypes);
            })).Value;
        }

        private IEnumerable<IFeatureInfo> GetAllUnorderedFeatures()
        {
            if (_allUnorderedFeatureInfos == null)
            {
                _allUnorderedFeatureInfos = new ConcurrentBag<IFeatureInfo>(GetExtensions().SelectMany(x => x.Features));
            }

            return _allUnorderedFeatureInfos;
        }

        private static string GetSourceFeatureNameForType(Type type, string extensionId)
        {
            foreach (OrchardFeatureAttribute featureAttribute in type.GetTypeInfo().GetCustomAttributes(typeof(OrchardFeatureAttribute), false))
            {
                return featureAttribute.FeatureName;
            }
            return extensionId;
        }
    }
}
