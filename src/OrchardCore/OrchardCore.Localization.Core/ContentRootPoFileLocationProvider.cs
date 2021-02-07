using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Localization.PortableObject
{
    /// <summary>
    /// provides a localization files from the content root folder.
    /// </summary>
    public class ContentRootPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly string _resourcesContainer;

        /// <summary>
        /// Creates a new instance of <see cref="ContentRootPoFileLocationProvider"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostEnvironment"/>.</param>
        /// <param name="localizationOptions">The <see cref="LocalizationOptions"/>.</param>
        public ContentRootPoFileLocationProvider(IHostEnvironment hostingEnvironment, IOptions<LocalizationOptions> localizationOptions)
        {
            _fileProvider = hostingEnvironment.ContentRootFileProvider;
            _resourcesContainer = localizationOptions.Value.ResourcesPath;
        }

        /// <inheritdocs />
        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            yield return _fileProvider.GetFileInfo(Path.Combine(_resourcesContainer, cultureName + ".po"));
        }
    }
}
