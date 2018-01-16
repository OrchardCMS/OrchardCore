using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.PortableObject
{
    public class ContentRootPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly string _resourcesContainer;

        public ContentRootPoFileLocationProvider(IHostingEnvironment hostingEnvironment, IOptions<LocalizationOptions> localizationOptions)
        {
            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _resourcesContainer = localizationOptions.Value.ResourcesPath;
        }

        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            yield return _fileProvider.GetFileInfo(Path.Combine(_resourcesContainer, cultureName + ".po"));
        }
    }
}
