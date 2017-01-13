using Microsoft.Extensions.Logging;

namespace Orchard.Environment.Extensions.Loaders
{
    public class AmbientExtensionLoader : IExtensionLoader
    {
        private readonly IExtensionLibraryService _extensionLibraryService;
        private readonly ILogger _logger;

        public AmbientExtensionLoader(
            IExtensionLibraryService extensionLibraryService,
            ILogger<AmbientExtensionLoader> logger)
        {
            _extensionLibraryService = extensionLibraryService;
            _logger = logger;
        }

        public string Name => GetType().Name;

        public int Order => 20;

        public ExtensionEntry Load(IExtensionInfo extensionInfo)
        {
            try
            {
                var assembly = _extensionLibraryService.LoadAmbientExtension(extensionInfo);

                if (assembly == null)
                {
                    return null;
                }

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Loaded referenced ambient extension \"{0}\": assembly name=\"{1}\"", extensionInfo.Id, assembly.FullName);
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