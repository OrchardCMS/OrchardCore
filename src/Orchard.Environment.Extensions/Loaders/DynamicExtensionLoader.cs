using Microsoft.Extensions.Logging;
using System.Linq;

namespace Orchard.Environment.Extensions.Loaders
{
    public class DynamicExtensionLoader : IExtensionLoader
    {
        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public DynamicExtensionLoader(
            IExtensionLibraryService extensionLibraryService,
            ILogger<DynamicExtensionLoader> logger)
        {
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 50;

        public ExtensionEntry Load(IExtensionInfo extensionInfo)
        {
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
                    Assembly = assembly
                };
            }
            catch
            {
                return null;
            }
       }
    }
}