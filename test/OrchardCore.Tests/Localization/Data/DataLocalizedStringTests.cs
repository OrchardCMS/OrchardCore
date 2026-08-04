namespace OrchardCore.Localization.Data.Tests;

public class DataLocalizedStringTests
{
    [Fact]
    public void DataLocalizedString_StringDataType_ReturnsValue()
    {
        // Arrange
        var dataLocalizedString = new DataLocalizedString("Content Types", "Blog", "Translated Blog");

        // Act
        string value = dataLocalizedString;

        // Assert
        Assert.Equal("Translated Blog", value);
    }

    [Fact]
    public void DataLocalizedString_VarDataType_ReturnsValue()
    {
        // Arrange
        var dataLocalizedString = new DataLocalizedString("Content Types", "Blog", "Translated Blog");

        // Act
        var value = dataLocalizedString;

        // Assert
        Assert.Equal("Translated Blog", value);
    }

    [Fact]
    public void DataLocalizedString_ToString_ReturnsValue()
    {
        // Arrange
        var dataLocalizedString = new DataLocalizedString("Content Types", "Blog", "Translated Blog");

        // Act
        var value = dataLocalizedString.ToString();

        // Assert
        Assert.Equal("Translated Blog", value);
    }

    [Fact]
    public void DataLocalizedString_Localizer_ReturnsValue()
    {
        // Arrange
        var context = "context";
        var culture = "it";
        var dataLocalizedString = new DataLocalizedString("Content Types", "Blog", "Blog");
        var dataTranslationProviderMock = new Mock<IDataTranslationProvider>();

        dataTranslationProviderMock
            .Setup(tp => tp.LoadTranslations(It.Is<string>(c => c == culture), It.IsAny<CultureDictionary>()))
            .Callback<string, CultureDictionary>((c, d) => d.MergeTranslations(new CultureDictionaryRecord[]
            {
               new CultureDictionaryRecord("Create {0}", context, [ "Creare {0}" ]),
            }));

        var memoryCache = new ServiceCollection()
            .AddMemoryCache()
            .BuildServiceProvider()
            .GetService<IMemoryCache>();

        var localizer = new DataLocalizer(
            new DataResourceManager(dataTranslationProviderMock.Object, memoryCache),
            fallBackToParentCulture: true,
            Mock.Of<ILogger>());

        CultureInfo.CurrentUICulture = new CultureInfo(culture);

        // Act
        var translation = localizer["Create {0}", context, dataLocalizedString];

        // Assert
        Assert.NotEqual("Creare Content Types.Blog", translation);
        Assert.Equal("Creare Blog", translation);
    }
}
