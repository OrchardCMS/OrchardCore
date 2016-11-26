using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private readonly ExtensionOptions _extensionOptions;
        private readonly IExtensionProvider _extensionProvider;
        private readonly IExtensionLoader _extensionLoader;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        private readonly ConcurrentDictionary<string, Task<ExtensionEntry>> _extensions
            = new ConcurrentDictionary<string, Task<ExtensionEntry>>();

        private readonly ConcurrentDictionary<string, Task<FeatureEntry>> _features
            = new ConcurrentDictionary<string, Task<FeatureEntry>>();

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
            return GetExtensions()[extensionId];
        }

        private IExtensionInfoList _extensionInfoList;

        public IExtensionInfoList GetExtensions()
        {
            if (_extensionInfoList == null)
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

                _extensionInfoList = new ExtensionInfoList(extensionsById.Values.ToList());
            }

            return _extensionInfoList;
        }

        public IFeatureInfoList GetFeatureDependencies(string featureId)
        {
            var features = GetExtensions().Features;

            var feature = features.FirstOrDefault(x => x.Id == featureId);
            if (feature == null)
            {
                return EmptyFeatureInfoList.Singleton;
            }

            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(features.Where(subject => feature.Dependencies.Any(x => x == subject.Id)).ToArray());

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(features.Where(subject => feature.Dependencies.Any(x => x == subject.Id)).ToArray());
                }
            }

            return new FeatureInfoList(dependencies.Distinct().ToList());
        }

        public IFeatureInfoList GetDependentFeatures(string featureId, IFeatureInfo[] featuresToSearch)
        {
            var features = GetExtensions().Features;

            var feature = features.FirstOrDefault(x => x.Id == featureId);
            if (feature == null)
            {
                return EmptyFeatureInfoList.Singleton;
            }

            var getDependants =
                new Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]>(
                    (currentFeature, fs) => fs
                        .Where(f =>
                                f.Dependencies.Any(dep => dep == currentFeature.Id)
                               ).ToArray());
            
            var dependentFeatures =
                GetDependentFeatures(feature, featuresToSearch, getDependants);

            return new FeatureInfoList(dependentFeatures);
        }

        private IList<IFeatureInfo> GetDependentFeatures(
            IFeatureInfo feature,
            IFeatureInfo[] features,
            Func<IFeatureInfo, IFeatureInfo[], IFeatureInfo[]> getAffectedDependencies)
        {
            var dependencies = new HashSet<IFeatureInfo>() { feature };
            var stack = new Stack<IFeatureInfo[]>();

            stack.Push(getAffectedDependencies(feature, features));

            while (stack.Count > 0)
            {
                var next = stack.Pop();
                foreach (var dependency in next.Where(dependency => !dependencies.Contains(dependency)))
                {
                    dependencies.Add(dependency);
                    stack.Push(getAffectedDependencies(dependency, features));
                }
            }

            return dependencies.ToList();
        }

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.
            return _extensions.GetOrAdd(extensionInfo.Id, async id =>
            {
                var extension = _extensionLoader.Load(extensionInfo);

                if (extension.IsError && L.IsEnabled(LogLevel.Warning))
                {
                    
                    L.LogError("No suitable loader found for extension \"{0}\"", id);
                }

                return await Task.FromResult(extension);
            });
        }

        public Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
        {
            return _features.GetOrAdd(feature.Id, async (id) =>
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
                    if (sourceFeature == id)
                    {
                        featureTypes.Add(type);
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }

                return new CompiledFeatureEntry(feature, featureTypes);
            });
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
