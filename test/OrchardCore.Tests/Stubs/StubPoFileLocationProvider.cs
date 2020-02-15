using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using OrchardCore.Tests;

namespace OrchardCore.Tests
{
    public class StubPoFileLocationProvider : ILocalizationFileLocationProvider
    {
        private readonly IFileProvider _fileProvider;
        private readonly string _resourcesContainer;

        public StubPoFileLocationProvider(IHostEnvironment hostingEnvironment, IOptions<LocalizationOptions> localizationOptions)
        {
            var rootPath = new DirectoryInfo(hostingEnvironment.ContentRootPath).Parent.Parent.Parent.FullName;
            _fileProvider = new PhysicalFileProvider(rootPath);
            _resourcesContainer = localizationOptions.Value.ResourcesPath;
        }

        public IEnumerable<IFileInfo> GetLocations(string cultureName)
        {
            yield return _fileProvider.GetFileInfo(Path.Combine(_resourcesContainer, cultureName + ".po"));
        }
    }
}