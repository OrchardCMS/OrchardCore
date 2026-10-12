namespace OrchardCore.Localization.Data.Tests;

public class DataLocalizerFactoryTests
{
    private readonly Mock<IDataTranslationProvider> _dataTranslationProviderMock;

    public DataLocalizerFactoryTests()
    {
        _dataTranslationProviderMock = new Mock<IDataTranslationProvider>();
    }

    [Fact]
    public void CreateDataLocalizer_Default_Succeeds()
    {
        // Arrange
        var context = "context";

        SetupDictionary("fr", new[] { new CultureDictionaryRecord("Hello", context, new[] { "Bonjour" }) });

        var memoryCache = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider()
            .GetService<IMemoryCache>();

        var dataResourceManager = new DataResourceManager(_dataTranslationProviderMock.Object, memoryCache);
        var requestlocalizationOptions = Mock.Of<IOptionsMonitor<RequestLocalizationOptions>>(monitor => monitor.CurrentValue == new RequestLocalizationOptions { FallBackToParentUICultures = true });
        var logger = Mock.Of<ILogger<DataLocalizerFactory>>();
        var localizerFactory = new DataLocalizerFactory(dataResourceManager, requestlocalizationOptions, logger);

        // Act
        var localizer = localizerFactory.Create();

        CultureInfo.CurrentUICulture = new CultureInfo("fr");

        // Assert
        Assert.NotNull(localizer);
        Assert.Single(localizer.GetAllStrings(context, includeParentCultures: false));
    }

    [Fact]
    public void Create_FallBackToParentCulturesChanged_UsesCurrentValue()
    {
        // Arrange
        var context = "context";

        SetupDictionary("fr", [new CultureDictionaryRecord("Hello", context, ["Bonjour"])]);

        var fallBackToParentCultures = false;
        var changeTokenSource = new RequestLocalizationOptionsChangeTokenSource();

        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .AddMemoryCache()
            .AddDataLocalization()
            .Replace(ServiceDescriptor.Singleton<IDataTranslationProvider>(_dataTranslationProviderMock.Object))
            .AddSingleton<IOptionsChangeTokenSource<RequestLocalizationOptions>>(changeTokenSource)
            .Configure<RequestLocalizationOptions>(options => options.FallBackToParentUICultures = fallBackToParentCultures)
            .BuildServiceProvider();

        var localizerFactory = serviceProvider.GetRequiredService<IDataLocalizerFactory>();

        using var cultureScope = CultureScope.Create("fr-CA");

        Assert.Equal("Hello", localizerFactory.Create()["Hello", context].Value);

        // Act
        fallBackToParentCultures = true;
        changeTokenSource.Change();

        // Assert
        Assert.Equal("Bonjour", localizerFactory.Create()["Hello", context].Value);
    }

    private sealed class RequestLocalizationOptionsChangeTokenSource : IOptionsChangeTokenSource<RequestLocalizationOptions>
    {
        private CancellationTokenSource _cancellationTokenSource = new();

        public string Name => Options.DefaultName;

        public IChangeToken GetChangeToken() => new CancellationChangeToken(_cancellationTokenSource.Token);

        public void Change()
        {
            var previous = Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource());
            previous.Cancel();
        }
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
