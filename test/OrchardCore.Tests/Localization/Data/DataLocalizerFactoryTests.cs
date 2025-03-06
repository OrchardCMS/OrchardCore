namespace OrchardCore.Localization.Data.Tests;

public class DataLocalizerFactoryTests
{
    private readonly Mock<IDataTranslationProvider> _dataTranslationProviderMock;

    public DataLocalizerFactoryTests()
    {
        _dataTranslationProviderMock = new Mock<IDataTranslationProvider>();
    }

    [Fact]
    public void CreateDataLocalizer()
    {
        // Arrange
        var context = "context";

        SetupDictionary("fr", new[] { new CultureDictionaryRecord("Hello", context, new[] { "Bonjour" }) });

        var memoryCache = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider()
            .GetService<IMemoryCache>();

        var dataResourceManager = new DataResourceManager(_dataTranslationProviderMock.Object, memoryCache);
        var requestlocalizationOptions = Options.Create(new RequestLocalizationOptions { FallBackToParentUICultures = true });
        var logger = Mock.Of<ILogger<DataLocalizerFactory>>();
        var localizerFactory = new DataLocalizerFactory(dataResourceManager, requestlocalizationOptions, logger);

        // Act
        var localizer = localizerFactory.Create();

        CultureInfo.CurrentUICulture = new CultureInfo("fr");

        // Assert
        Assert.NotNull(localizer);
        Assert.Single(localizer.GetAllStrings(context, includeParentCultures: false));
    }

    private void SetupDictionary(string cultureName, IEnumerable<CultureDictionaryRecord> records)
    {
        _dataTranslationProviderMock
            .Setup(tp => tp.LoadTranslations(It.Is<string>(c => c == cultureName), It.IsAny<CultureDictionary>()))
            .Callback<string, CultureDictionary>((c, d) =>
            {
                d.MergeTranslations(records);
            });
    }
}
