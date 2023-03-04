using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;

namespace OrchardCore.Testing.Stubs;

public class PoFileLocationProviderStub : ILocalizationFileLocationProvider
{
    private readonly IFileProvider _fileProvider;
    private readonly string _resourcesPath;

    public PoFileLocationProviderStub(IHostEnvironment hostingEnvironment, IOptions<LocalizationOptions> localizationOptions)
    {
        var rootPath = new DirectoryInfo(hostingEnvironment.ContentRootPath).Parent.Parent.Parent.FullName;
        _fileProvider = new PhysicalFileProvider(rootPath);
        _resourcesPath = localizationOptions.Value.ResourcesPath;
    }

    public IEnumerable<IFileInfo> GetLocations(string cultureName)
    {
        var resourcePath = Path.Combine(_resourcesPath, cultureName + ".po");

        yield return _fileProvider.GetFileInfo(resourcePath);
    }
}
