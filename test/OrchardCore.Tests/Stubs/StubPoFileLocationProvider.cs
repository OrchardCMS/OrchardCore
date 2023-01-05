using OrchardCore.Localization;

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
