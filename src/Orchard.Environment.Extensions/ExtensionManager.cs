using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Features;
using Orchard.Environment.Extensions.Loaders;
using Orchard.Services;
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
        private readonly ILogger _logger;

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
            _logger = logger;
            T = localizer;
        }
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

        public Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.
            return _extensions.GetOrAdd(extensionInfo.Id, async id =>
            {
                var extension = _extensionLoader.Load(extensionInfo);

                if (extension.IsError && _logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning("No suitable loader found for extension \"{0}\"", id);
                }

                return await Task.FromResult(extension);
            });
        }

        public async Task<IEnumerable<ExtensionEntry>> LoadExtensionsAsync(IEnumerable<IExtensionInfo> extensionInfos) {
            var entries = new List<ExtensionEntry>();

            foreach (var extensionInfo in extensionInfos) {
                var extension = await LoadExtensionAsync(extensionInfo);
                entries.Add(extension);
            }

            return entries;
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
                    string sourceFeature = GetSourceFeatureNameForType(type, feature.Extension.Id);
                    if (sourceFeature == id)
                    {
                        featureTypes.Add(type);
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }

                return new CompiledFeatureEntry(feature, featureTypes);
            });
        }

        public async Task<IEnumerable<FeatureEntry>> LoadFeaturesAsync(IEnumerable<IFeatureInfo> features)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Loading features");
            }

            var featuresToReturn =
                new List<FeatureEntry>();

            foreach (var feature in features)
            {
                var featureToReturn = await LoadFeatureAsync(feature);
                featuresToReturn.Add(featureToReturn);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Done loading features");
            }

            return featuresToReturn;
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
