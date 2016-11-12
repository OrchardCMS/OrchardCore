using Cache;
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

        private readonly AsyncCache _extensions;
        private readonly AsyncCache _features;

        private readonly ConcurrentDictionary<string, IExtensionInfo> _extensionsById
            = new ConcurrentDictionary<string, IExtensionInfo>();

        public ExtensionManager(
            IOptions<ExtensionOptions> optionsAccessor,
            IEnumerable<IExtensionProvider> extensionProviders,
            IEnumerable<IExtensionLoader> extensionLoaders,
            IClock clock,
            IHostingEnvironment hostingEnvironment,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<ExtensionManager> logger,
            IStringLocalizer<ExtensionManager> localizer)
        {
            _extensionOptions = optionsAccessor.Value;
            _extensionProvider = new CompositeExtensionProvider(extensionProviders);
            _extensionLoader = new CompositeExtensionLoader(extensionLoaders);

            _extensions = new AsyncCache(() => clock.UtcNow, TimeSpan.FromHours(1));
            _features = new AsyncCache(() => clock.UtcNow, TimeSpan.FromHours(1));

            _hostingEnvironment = hostingEnvironment;
            _typeFeatureProvider = typeFeatureProvider;
            _logger = logger;
            T = localizer;
        }
        public IStringLocalizer T { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            InitializeExtensions();

            return _extensionsById.ContainsKey(extensionId) ? _extensionsById[extensionId] : null;
        }

        public IExtensionInfoList GetExtensions()
        {
            InitializeExtensions();

            return new ExtensionInfoList(_extensionsById);
        }

        public async Task<ExtensionEntry> LoadExtensionAsync(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.

            try
            {
                return await _extensions.Get(extensionInfo.Id, id =>
                {
                    var extension = _extensionLoader.Load(extensionInfo);
                    
                    if (extension == null && _logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning("No suitable loader found for extension \"{0}\"", extensionInfo.Id);
                    }

                    return Task.FromResult(extension);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(string.Format("Error loading extension '{0}'", extensionInfo.Id), ex);
                throw new OrchardException(T["Error while loading extension '{0}'.", extensionInfo.Id], ex);
            }
        }

        public async Task<IEnumerable<ExtensionEntry>> LoadExtensionsAsync(IEnumerable<IExtensionInfo> extensionInfos) {
            var extensionEntries = new ConcurrentBag<ExtensionEntry>();

            Parallel.ForEach(extensionInfos, async extensionInfo =>
            {
                try
                {
                    var extension = await LoadExtensionAsync(extensionInfo);
                    extensionEntries.Add(extension);
                }
                catch (Exception e)
                {
                    extensionEntries.Add(new FailedExtensionEntry { Exception = e, ExtensionInfo = extensionInfo });
                }
            });

            return await Task.FromResult(extensionEntries);
        }

        public async Task<FeatureEntry> LoadFeatureAsync(IFeatureInfo feature)
        {
            return await _features.Get(feature.Id, (key) =>
            {
                var loadedExtension = LoadExtensionAsync(feature.Extension).Result;

                if (loadedExtension == null)
                {
                    return Task.FromResult<FeatureEntry>(new NonCompiledFeatureEntry(feature));
                }

                var extensionTypes = loadedExtension
                    .ExportedTypes
                    .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract);

                var featureTypes = new List<Type>();

                foreach (var type in extensionTypes)
                {
                    string sourceFeature = GetSourceFeatureNameForType(type, feature.Extension.Id);
                    if (sourceFeature == feature.Id)
                    {
                        featureTypes.Add(type);
                        _typeFeatureProvider.TryAdd(type, feature);
                    }
                }

                return Task.FromResult<FeatureEntry>(new CompiledFeatureEntry(feature, featureTypes));
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

        private void InitializeExtensions()
        {
            foreach (var searchPath in _extensionOptions.SearchPaths)
            {
                foreach (var subDirectory in _hostingEnvironment
                    .ContentRootFileProvider
                    .GetDirectoryContents(searchPath)
                    .Where(x => x.IsDirectory))
                {
                    var extensionId = subDirectory.Name;

                    if (!_extensionsById.ContainsKey(extensionId))
                    {
                        var subPath = Path.Combine(searchPath, extensionId);

                        var extensionInfo =
                            _extensionProvider.GetExtensionInfo(subPath);

                        if (extensionInfo != null)
                        {
                            _extensionsById.TryAdd(extensionId, extensionInfo);
                        }
                    }
                }
            }
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
