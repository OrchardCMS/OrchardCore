using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Localization
{
    public class ModularPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private const string PoFileName = "OrchardCore.po";
        private const string ExtensionDataFolder = "App_Data";

        private readonly IExtensionManager _extensionsManager;
        private readonly string _root;
        private readonly string _resourcesContainer;
        private readonly string _applicationDataContainer;
        private readonly string _shellDataContainer;

        public ModularPoFileLocationProvider(
            IExtensionManager extensionsManager,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            IOptions<LocalizationOptions> localizationOptions,
            ShellSettings shellSettings)
        {
            _extensionsManager = extensionsManager;

            _root = hostingEnvironment.ContentRootPath;
            _resourcesContainer = localizationOptions.Value.ResourcesPath; // Localization
            _applicationDataContainer = shellOptions.Value.ShellsApplicationDataPath;
            _shellDataContainer = Path.Combine(_applicationDataContainer, shellOptions.Value.ShellsContainerName, shellSettings.Name);
        }

        public IEnumerable<string> GetLocations(string cultureName)
        {
            // Load .po files in each extension folder first, based on the extensions order
            foreach (var extension in _extensionsManager.GetExtensions())
            {
                yield return Path.Combine(_root, extension.SubPath, ExtensionDataFolder, _resourcesContainer, cultureName, PoFileName);
            }

            // Then load global .po file for the application
            yield return Path.Combine(_applicationDataContainer, _resourcesContainer, cultureName, PoFileName);

            // Finally load tenant-specific .po file
            yield return Path.Combine(_shellDataContainer, _resourcesContainer, cultureName, PoFileName);
        }
    }
}
