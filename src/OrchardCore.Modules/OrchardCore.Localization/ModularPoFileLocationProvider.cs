using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Localization
{
    public class ModularPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private const string PoFileName = "OrchardCore.po";

        private readonly IExtensionManager _extensionsManager;
        private readonly IFileProvider _fileProvider;
        private readonly string _resourcesContainer;
        private readonly string _shellContainer;
        private readonly string _shellName;

        public ModularPoFileLocationProvider(
            IExtensionManager extensionsManager,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            IOptions<LocalizationOptions> localizationOptions,
            ShellSettings shellSettings)
        {
            _extensionsManager = extensionsManager;

            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _resourcesContainer = localizationOptions.Value.ResourcesPath; // Localization
            _shellContainer = shellOptions.Value.ShellsContainerName;
            _shellName = shellSettings.Name;
        }

        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            // Load .po files in each extension folder first, based on the extensions order
            foreach (var extension in _extensionsManager.GetExtensions())
            {
                yield return _fileProvider.GetFileInfo(Path.Combine(extension.SubPath, _resourcesContainer, cultureName, PoFileName));
            }

            // Then load global .po file for the applications
            yield return _fileProvider.GetFileInfo(Path.Combine(_resourcesContainer, cultureName, PoFileName));

            // Finally load tenant-specific .po file
            yield return _fileProvider.GetFileInfo(Path.Combine(_shellContainer, _shellName, _resourcesContainer, cultureName, PoFileName));
        }
    }
}
