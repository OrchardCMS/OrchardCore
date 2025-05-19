namespace OrchardCore.Localization.Data.Tests;

public class DataLocalizerTests
{
    private readonly Mock<IDataTranslationProvider> _dataTranslationProviderMock;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    private readonly string _context = "context";

    public DataLocalizerTests()
    {
        _dataTranslationProviderMock = new Mock<IDataTranslationProvider>();
        _memoryCache = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider()
            .GetService<IMemoryCache>();

        _logger = Mock.Of<ILogger>();
    }

    [Fact]
    public void LocalizerReturnsTranslationsFromProvidedDictionary()
    {
        // Arrange
        SetupDictionary("fr", new CultureDictionaryRecord("Hello", _context, new[] { "Bonjour" }));

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture: true, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("fr");

        // Act
        var translation = localizer["Hello", _context];

        // Assert
        Assert.Equal("Bonjour", translation);
    }

    [Fact]
    public void LocalizerReturnsOriginalTextIfTranslationsDoesntExistInProvidedDictionary()
    {
        // Arrange
        SetupDictionary("fr", new CultureDictionaryRecord("Hello", _context, new[] { "Bonjour" }));

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture: true, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("fr");

        // Act
        var translation = localizer["Bye", _context];

        // Assert
        Assert.Equal("Bye", translation);
    }

    [Fact]
    public void LocalizerReturnsOriginalTextIfDictionaryIsEmpty()
    {
        // Arrange
        SetupDictionary("fr", Array.Empty<CultureDictionaryRecord>());

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture: true, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("fr");

        // Act
        var translation = localizer["Hello", _context];

        // Assert
        Assert.Equal("Hello", translation);
    }

    [Fact]
    public void LocalizerFallbacksToParentCultureIfTranslationDoesntExistInSpecificCulture()
    {
        // Arrange
        SetupDictionary("fr", new CultureDictionaryRecord("Hello", _context, new[] { "Bonjour" }));

        SetupDictionary("fr-FR", new CultureDictionaryRecord("Bye", _context, new[] { "au revoir" }));

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture: true, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

        // Act
        var translation = localizer["Hello", _context];

        // Assert
        Assert.Equal("Bonjour", translation);
    }

    [Fact]
    public void LocalizerReturnsTranslationFromSpecificCultureIfItExists()
    {
        // Arrange
        SetupDictionary("fr", new CultureDictionaryRecord("Hello", _context, new[] { "Bonjour" }));

        SetupDictionary("fr-FR", new CultureDictionaryRecord("Bye", _context, new[] { "au revoir" }));

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture: true, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

        // Act
        var translation = localizer["Bye", _context];

        // Assert
        Assert.Equal("au revoir", translation);
    }

    [Fact]
    public void LocalizerReturnsFormattedTranslation()
    {
        // Arrange
        SetupDictionary("cs", new CultureDictionaryRecord("The page (ID:{0}) was deleted.", _context, new[] { "Stránka (ID:{0}) byla smazána." }));

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture: true, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("cs");

        // Act
        var translation = localizer["The page (ID:{0}) was deleted.", _context, 1];

        // Assert
        Assert.Equal("Stránka (ID:1) byla smazána.", translation);
    }

    [Theory]
    [InlineData(false, "hello", "hello")]
    [InlineData(true, "hello", "مرحبا")]
    public void LocalizerFallBackToParentCultureIfFallBackToParentUICulturesIsTrue(bool fallBackToParentCulture, string resourceKey, string expected)
    {
        // Arrange
        SetupDictionary("ar", new CultureDictionaryRecord("hello", _context, new[] { "مرحبا" }));
        SetupDictionary("ar-YE", Array.Empty<CultureDictionaryRecord>());

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, _memoryCache), fallBackToParentCulture, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("ar-YE");

        // Act
        var translation = localizer[resourceKey, _context];

        // Assert
        Assert.Equal(expected, translation);
    }

    [Theory]
    [InlineData(false, new[] { "مدونة", "منتج" })]
    [InlineData(true, new[] { "مدونة", "منتج", "قائمة", "صفحة", "مقالة" })]
    public void LocalizerReturnsGetAllStrings(bool includeParentCultures, string[] expected)
    {
        // Arrange
        SetupDictionary("ar", new CultureDictionaryRecord[]
        {
                new("Blog", _context, new[] { "مدونة" }),
                new("Menu", _context, new[] { "قائمة" }),
                new("Page", _context, new[] { "صفحة" }),
                new("Article", _context, new[] { "مقالة" }),
        });

        SetupDictionary("ar-YE", new CultureDictionaryRecord[]
        {
                new("Blog", _context, new[] { "مدونة" }),
                new("Product", _context, new[] { "منتج" }),
        });

        var localizer = new DataLocalizer(new DataResourceManager(_dataTranslationProviderMock.Object, GetMemoryCache()), includeParentCultures, _logger);

        CultureInfo.CurrentUICulture = new CultureInfo("ar-YE");

        // Act
        var translations = localizer
            .GetAllStrings(_context, includeParentCultures)
            .Select(l => l.Value).ToArray();

        // Assert
        Assert.Equal(expected.Length, translations.Length);
    }

    private static IMemoryCache GetMemoryCache()
    {
        var serviceProvider = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider();

        return serviceProvider.GetService<IMemoryCache>();
    }

    private void SetupDictionary(string cultureName, CultureDictionaryRecord record)
        => SetupDictionary(cultureName, new CultureDictionaryRecord[] { record });

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
