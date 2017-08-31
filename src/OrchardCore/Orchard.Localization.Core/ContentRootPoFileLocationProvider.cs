using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Orchard.Localization.PortableObject
{
    public class ContentRootPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly string _root;
        private readonly string _resourcesContainer;

        public ContentRootPoFileLocationProvider(IHostingEnvironment hostingEnvironment, IOptions<LocalizationOptions> localizationOptions)
        {
            _root = hostingEnvironment.ContentRootPath;
            _resourcesContainer = localizationOptions.Value.ResourcesPath;
        }

        public IEnumerable<string> GetLocations(string cultureName)
        {
            yield return Path.Combine(_root, _resourcesContainer, cultureName + ".po");
        }
    }
}
