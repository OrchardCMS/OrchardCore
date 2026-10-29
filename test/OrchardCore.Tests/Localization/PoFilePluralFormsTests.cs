using System.Text;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.Tests.Localization;

public class PoFilePluralFormsTests
{
    // The 'fr' rule of the default provider has three forms (one, other, many), while this file has two.
    private const string FrenchWithHeader =
        """
        msgid ""
        msgstr ""
        "Language: fr\n"
        "Plural-Forms: nplurals=2; plural=(n > 1);\n"

        msgid "1 ball"
        msgid_plural "{0} balls"
        msgstr[0] "{0} balle"
        msgstr[1] "{0} balles"
        """;

    private const string FrenchWithoutHeader =
        """
        msgid "1 car"
        msgid_plural "{0} cars"
        msgstr[0] "{0} voiture"
        msgstr[1] "{0} voitures"
        msgstr[2] "{0} de voitures"
        """;

    [Theory]
    [InlineData(0, "0 balle")]
    [InlineData(1, "1 balle")]
    [InlineData(2, "2 balles")]
    [InlineData(1000000, "1000000 balles")]
    public void Plural_PoFileWithPluralFormsHeader_UsesHeaderRule(int count, string expected)
    {
        var localizer = CreateLocalizer();

        using (CultureScope.Create("fr"))
        {
            Assert.Equal(expected, localizer.Plural(count, "1 ball", "{0} balls", count).Value);
        }
    }

    [Theory]
    [InlineData(1, "1 voiture")]
    [InlineData(2, "2 voitures")]
    [InlineData(1000000, "1000000 de voitures")]
    public void Plural_PoFileWithoutPluralFormsHeader_UsesProviderRule(int count, string expected)
    {
        var localizer = CreateLocalizer();

        using (CultureScope.Create("fr"))
        {
            Assert.Equal(expected, localizer.Plural(count, "1 car", "{0} cars", count).Value);
        }
    }

    private static PortableObjectStringLocalizer CreateLocalizer()
    {
        var locationProvider = new Mock<ILocalizationFileLocationProvider>();
        locationProvider
            .Setup(o => o.GetLocations("fr"))
            .Returns([CreateFile(FrenchWithHeader), CreateFile(FrenchWithoutHeader)]);

        var localizationManager = new LocalizationManager(
            [new DefaultPluralRuleProvider()],
            [new PoFilesTranslationsProvider(locationProvider.Object)],
            new MemoryCache(new MemoryCacheOptions()));

        return new PortableObjectStringLocalizer(null, localizationManager, false, Mock.Of<ILogger>());
    }

    private static IFileInfo CreateFile(string content)
    {
        var file = new Mock<IFileInfo>();
        file.SetupGet(o => o.Exists).Returns(true);
        file.SetupGet(o => o.IsDirectory).Returns(false);
        file.Setup(o => o.CreateReadStream()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes(content)));

        return file.Object;
    }
}
