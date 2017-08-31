using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Orchard.Environment.Extensions;
using Orchard.Environment.Shell;
using Orchard.Localization.Abstractions;

namespace Orchard.Localization.PortableObject
{
    public class DefaultPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly IExtensionManager _extensionsManager;
        private readonly string _root;
        private readonly string _rootContainer;
        private readonly string _resourcesContainer;
        private readonly string _shellContainer;
        private readonly string _shellName;

        public DefaultPoFileLocationProvider(
            IExtensionManager extensionsManager,
            IHostingEnvironment hostingEnvironment,
            IOptions<ShellOptions> shellOptions,
            IOptions<LocalizationOptions> localizationOptions,
            ShellSettings shellSettings)
        {
            _extensionsManager = extensionsManager;

            _root = hostingEnvironment.ContentRootPath;
            _rootContainer = shellOptions.Value.ShellsRootContainerName;
            _resourcesContainer = localizationOptions.Value.ResourcesPath;
            _shellContainer = shellOptions.Value.ShellsContainerName;
            _shellName = shellSettings.Name;
        }

        public IEnumerable<string> GetLocations(string cultureName)
        {
            // Load .po files in each extension folder first, based on the extensions order
            foreach (var extension in _extensionsManager.GetExtensions())
            {
                yield return Path.Combine(_root, extension.SubPath, "App_Data", "Localization", cultureName, "orchard.po");
            }

            // Then load global .po file for the applications
            yield return Path.Combine(_root, _rootContainer, _resourcesContainer, cultureName, "orchard.po");

            // Finally load tenant-specific .po file
            yield return Path.Combine(_root, _rootContainer, _shellContainer, _shellName, _resourcesContainer, cultureName, "orchard.po");
        }
    }
}
