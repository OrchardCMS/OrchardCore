using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization;

namespace OrchardCore.Setup
{
    public class ModularPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private const string PoFileExtension = ".po";
        private const string ExtensionDataFolder = "App_Data";
        private const string CultureDelimiter = "-";

        private readonly IExtensionManager _extensionsManager;
        private readonly IFileProvider _fileProvider;
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

            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _resourcesContainer = localizationOptions.Value.ResourcesPath; // Localization
            _applicationDataContainer = shellOptions.Value.ShellsApplicationDataPath;
            _shellDataContainer = PathExtensions.Combine(_applicationDataContainer, shellOptions.Value.ShellsContainerName, shellSettings.Name);
        }

        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            var poFileName = cultureName + PoFileExtension;
            var extensions = _extensionsManager.GetExtensions();

            // Load .po files in each extension folder first, based on the extensions order
            foreach (var extension in extensions)
            {
                // From [Extension]/Localization
                yield return _fileProvider.GetFileInfo(PathExtensions.Combine(extension.SubPath, _resourcesContainer, poFileName));
            }

            // Load global .po files from /Localization
            yield return _fileProvider.GetFileInfo(PathExtensions.Combine(_resourcesContainer, poFileName));

            // Load tenant-specific .po file from /App_Data/Sites/[Tenant]/Localization
            yield return new PhysicalFileInfo(new FileInfo(PathExtensions.Combine(_shellDataContainer, _resourcesContainer, poFileName)));

            // Load each modules .po file for extending localization when using Orchard Core as a Nuget package
            foreach (var extension in extensions)
            {
                // \src\OrchardCore.Cms.Web\Localization\OrchardCore.Cms.Web\fr-CA.po
                yield return _fileProvider.GetFileInfo(PathExtensions.Combine(_resourcesContainer, extension.Id, poFileName));

                // \src\OrchardCore.Cms.Web\Localization\OrchardCore.Cms.Web-fr-CA.po
                yield return _fileProvider.GetFileInfo(PathExtensions.Combine(_resourcesContainer, extension.Id + CultureDelimiter + poFileName));

                // \src\OrchardCore.Cms.Web\Localization\fr-CA\OrchardCore.Cms.Web.po
                yield return _fileProvider.GetFileInfo(PathExtensions.Combine(_resourcesContainer, cultureName, extension.Id + PoFileExtension));
            }
        }
    }
}
