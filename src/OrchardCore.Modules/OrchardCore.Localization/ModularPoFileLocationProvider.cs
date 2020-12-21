using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a localization provider to provide the locations of the modules PO files.
    /// </summary>
    public class ModularPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private const string PoFileExtension = ".po";
        private const string CultureDelimiter = "-";

        private readonly IExtensionManager _extensionsManager;
        private readonly IFileProvider _fileProvider;
        private readonly string _resourcesContainer;
        private readonly string _applicationDataContainer;
        private readonly string _shellDataContainer;

        /// <summary>
        /// Creates a new intance of the <see cref="ModularPoFileLocationProvider"/>.
        /// </summary>
        /// <param name="extensionsManager">The <see cref="IExtensionManager"/>.</param>
        /// <param name="hostingEnvironment">The <see cref="IHostEnvironment"/>.</param>
        /// <param name="shellOptions">The <see cref="ShellOptions"/>.</param>
        /// <param name="localizationOptions">The <see cref="LocalizationOptions"/>.</param>
        /// <param name="shellSettings">The <see cref="ShellSettings"/>.</param>
        public ModularPoFileLocationProvider(
            IExtensionManager extensionsManager,
            IHostEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            IOptions<LocalizationOptions> localizationOptions,
            ShellSettings shellSettings)
        {
            _extensionsManager = extensionsManager;

            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _resourcesContainer = localizationOptions.Value.ResourcesPath;
            _applicationDataContainer = shellOptions.Value.ShellsApplicationDataPath;
            _shellDataContainer = PathExtensions.Combine(_applicationDataContainer, shellOptions.Value.ShellsContainerName, shellSettings.Name);
        }

        /// <inheritdocs />
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

            // Load each modules .po file for extending localization when using Orchard Core as a NuGet package
            foreach (var extension in extensions)
            {
                // \src\OrchardCore.Cms.Web\Localization\OrchardCore.Cms.Web\fr-CA.po
                yield return _fileProvider.GetFileInfo(PathExtensions.Combine(_resourcesContainer, extension.Id, poFileName));

                // \src\OrchardCore.Cms.Web\Localization\OrchardCore.Cms.Web-fr-CA.po
                yield return _fileProvider.GetFileInfo(PathExtensions.Combine(_resourcesContainer, extension.Id + CultureDelimiter + poFileName));
            }

            // Load all .po files from a culture specific folder
            // e.g, \src\OrchardCore.Cms.Web\Localization\fr-CA\*.po
            foreach (var file in _fileProvider.GetDirectoryContents(PathExtensions.Combine(_resourcesContainer, cultureName)))
            {
                yield return file;
            }
        }
    }
}
