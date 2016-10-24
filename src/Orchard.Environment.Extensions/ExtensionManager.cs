using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions.Loaders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions
{
    public class ExtensionManager : IExtensionManager
    {
        private ExtensionOptions _extensionOptions;
        private IExtensionProvider _extensionProvider;
        private IEnumerable<IExtensionLoader> _extensionLoaders;
        private IHostingEnvironment _hostingEnvironment;
        private readonly ILogger _logger;

        // TODO (ngm) value providers not thread safe...
        private readonly ConcurrentDictionary<string, ExtensionEntry> _extensions 
            = new ConcurrentDictionary<string, ExtensionEntry>();

        public ExtensionManager(
            IOptions<ExtensionOptions> optionsAccessor,
            IExtensionProvider extensionProvider,
            IEnumerable<IExtensionLoader> extensionLoaders,
            IHostingEnvironment hostingEnvironment,
            ILogger<ExtensionManager> logger,
            IStringLocalizer<ExtensionManager> localizer)
        {
            _extensionOptions = optionsAccessor.Value;
            _extensionProvider = extensionProvider;
            _extensionLoaders = extensionLoaders;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
            T = localizer;
        }
        public IStringLocalizer T { get; set; }

        public IExtensionInfo GetExtension(string extensionId)
        {
            foreach (var searchPath in _extensionOptions.SearchPaths)
            {
                var subPath = 
                    Path.Combine(searchPath, extensionId);
                var extensionInfo = 
                    _extensionProvider.GetExtensionInfo(subPath);

                if (extensionInfo != null)
                {
                    return extensionInfo;
                }
            }

            return null;
        }

        public IExtensionInfoList GetExtensions()
        {
            // TODO (ngm) throw this to a static, no need to build this everytime
            IDictionary<string, IExtensionInfo> extensionsById
                = new Dictionary<string, IExtensionInfo>();

            foreach (var searchPath in _extensionOptions.SearchPaths)
            {
                foreach (var subDirectory in _hostingEnvironment
                    .ContentRootFileProvider
                    .GetDirectoryContents(searchPath).Where(x => x.IsDirectory))
                {
                    var extensionId = subDirectory.Name;
                    if (!extensionsById.ContainsKey(extensionId))
                    {
                        var subPath = Path.Combine(searchPath, extensionId);

                        var extensionInfo =
                            _extensionProvider.GetExtensionInfo(subPath);

                        if (extensionInfo != null)
                        {
                            extensionsById.Add(extensionId, extensionInfo);
                        }
                    }
                }
            }

            return new ExtensionInfoList(extensionsById);
        }

        public ExtensionEntry LoadExtension(IExtensionInfo extensionInfo)
        {
            // Results are cached so that there is no mismatch when loading an assembly twice.
            // Otherwise the same types would not match.

            try
            {
                return _extensions.GetOrAdd(extensionInfo.Id, id =>
                {
                    foreach (var loader in _extensionLoaders)
                    {
                        var entry = loader.Load(extensionInfo);
                        if (entry != null)
                        {
                            return entry;
                        }
                    }

                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning("No suitable loader found for extension \"{0}\"", extensionInfo.Id);
                    }

                    return null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(string.Format("Error loading extension '{0}'", extensionInfo.Id), ex);
                throw new OrchardException(T["Error while loading extension '{0}'.", extensionInfo.Id], ex);
            }
        }

        public IEnumerable<ExtensionEntry> LoadExtensions(IEnumerable<IExtensionInfo> extensionInfos) {
            List<ExtensionEntry> extensionEntries = new List<ExtensionEntry>();

            Parallel.ForEach(extensionInfos, extension =>
            {
                try
                {
                    extensionEntries.Add(LoadExtension(extension));
                }
                catch (Exception e)
                {
                    extensionEntries.Add(new FailedExtensionEntry { Exception = e, ExtensionInfo = extension });
                }
            });

            return extensionEntries;
        }
    }
}
