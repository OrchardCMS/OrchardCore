using Microsoft.Extensions.Logging;

namespace Orchard.Environment.Extensions.Loaders
{
    public class PrecompiledExtensionLoader : IExtensionLoader
    {
        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public PrecompiledExtensionLoader(
            IExtensionLibraryService extensionLibraryService,
            ILogger<PrecompiledExtensionLoader> logger)
        {
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 30;

        public ExtensionEntry Load(IExtensionInfo extensionInfo)
        {
            try
            {
                var assembly = _extensionLibraryService.LoadPrecompiledExtension(extensionInfo);
            
                if (assembly == null)
                {
                    return null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced precompiled extension \"{0}\": assembly name=\"{1}\"", extensionInfo.Id, assembly.FullName);
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