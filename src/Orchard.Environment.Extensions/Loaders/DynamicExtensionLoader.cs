using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;

namespace Orchard.Environment.Extensions.Loaders
{
    public class DynamicExtensionLoader : IExtensionLoader
    {
        private readonly string[] ExtensionsSearchPaths;

        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public DynamicExtensionLoader(
            IOptions<ExtensionOptions> optionsAccessor,
            IExtensionLibraryService extensionLibraryService,
            ILogger<DynamicExtensionLoader> logger)
        {
            ExtensionsSearchPaths = optionsAccessor.Value.SearchPaths.ToArray();
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 50;

        public ExtensionEntry Load(IExtensionInfo extensionInfo)
        {
            if (!ExtensionsSearchPaths.Contains(extensionInfo.ExtensionFileInfo.PhysicalPath))
            {
                return null;
            }

            try
            {
                var assembly = _extensionLibraryService.LoadDynamicExtension(extensionInfo);
            
                if (assembly == null)
                {
                    return null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced dynamic extension \"{0}\": assembly name=\"{1}\"", extensionInfo.Id, assembly.FullName);
                }

                return new ExtensionEntry
                {
                    ExtensionInfo = extensionInfo,
                    Assembly = assembly,
                    ExportedTypes = assembly.ExportedTypes
                };
            }
            catch
            {
                return null;
            }
       }
    }
}