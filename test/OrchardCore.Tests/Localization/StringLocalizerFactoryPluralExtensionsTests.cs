using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.Tests.Localization;

public class StringLocalizerFactoryPluralExtensionsTests
{
    private static readonly string s_context = typeof(StringLocalizerFactoryPluralExtensionsTests).FullName;

    [Fact]
    public void Plural_WithNullFactory_Throws()
    {
        Assert.Throws<ArgumentNullException>("factory", () => ((IStringLocalizerFactory)null).Plural(1, LocalizationSource.Create("item"), "items"));
    }

    [Fact]
    public void Plural_WithNullSource_ReturnsNull()
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);

        Assert.Null(factory.Object.Plural(1, null, "items"));
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_WithNullPlural_Throws()
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);

        Assert.Throws<ArgumentNullException>("plural", () => factory.Object.Plural(1, LocalizationSource.Create("item"), null));
        factory.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(1, "1 item")]
    [InlineData(0, "0 items")]
    [InlineData(2, "2 items")]
    public void Plural_WithoutContext_SelectsEnglishFormAndPrependsCount(int count, string expected)
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        var source = LocalizationSource.Create("{0} item");

        var result = factory.Object.Plural(count, source, "{0} items");

        Assert.Equal(expected, result.Value);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_WithoutContextAndArguments_PrependsCountBeforeArguments()
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        var source = LocalizationSource.Create("{0} item for {1}");

        var result = factory.Object.Plural(3, source, "{0} items for {1}", "Mike");

        Assert.Equal("3 items for Mike", result.Value);
        Assert.Equal("{0} item for {1}", source.Value);
        Assert.Null(source.Type);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_WithContext_DelegatesToLocalizerPluralUsingSourceType()
    {
        var source = LocalizationSource.Create<StringLocalizerFactoryPluralExtensionsTests>("item");
        var localizer = new Mock<IStringLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["item", It.Is<PluralizationArgument>(a => a.Count == 2 && a.Forms[0] == "item" && a.Forms[1] == "items")])
            .Returns(new LocalizedString("item", "2 translated-items"));
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var result = factory.Object.Plural(2, source, "items");

        Assert.Equal("2 translated-items", result.Value);
        factory.Verify(f => f.Create(source.Type), Times.Once);
        localizer.VerifyAll();
        localizer.VerifyNoOtherCalls();
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_SourceImmutability_DoesNotChangeAfterUse()
    {
        var source = LocalizationSource.Create<StringLocalizerFactoryPluralExtensionsTests>("{0} item");
        var localizer = new Mock<IStringLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["{0} item", It.IsAny<PluralizationArgument>()]).Returns(new LocalizedString("{0} item", "2 items"));
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        factory.Object.Plural(2, source, "{0} items");

        Assert.Equal("{0} item", source.Value);
        Assert.Equal(typeof(StringLocalizerFactoryPluralExtensionsTests), source.Type);
    }

    [Theory]
    [InlineData(1, "1 элемент")]
    [InlineData(2, "2 элемента")]
    [InlineData(5, "5 элементов")]
    [InlineData(0, "0 элементов")]
    [InlineData(21, "21 элемент")]
    [InlineData(22, "22 элемента")]
    public void Plural_WithContext_UsesActualFactoryAndFollowsCulturePluralRules(int count, string expected)
    {
        var localizationManager = new Mock<ILocalizationManager>();
        var dictionary = new CultureDictionary("ru", PluralizationRule.Russian);
        dictionary.MergeTranslations([
            new CultureDictionaryRecord("{0} item", s_context, ["{0} элемент", "{0} элемента", "{0} элементов"]),
        ]);
        localizationManager.Setup(m => m.GetDictionary(It.Is<CultureInfo>(c => c.Name == "ru"))).Returns(dictionary);

        var factory = new PortableObjectStringLocalizerFactory(
            localizationManager.Object,
            Options.Create(new RequestLocalizationOptions()),
            NullLogger<PortableObjectStringLocalizerFactory>.Instance);

        using var cultureScope = CultureScope.Create("ru");
        var source = LocalizationSource.Create<StringLocalizerFactoryPluralExtensionsTests>("{0} item");

        var result = factory.Plural(count, source, "{0} items");

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void Plural_WithContextAndAdditionalArguments_FormatsCountAndArguments()
    {
        var localizationManager = new Mock<ILocalizationManager>();
        var dictionary = new CultureDictionary("ru", PluralizationRule.Russian);
        dictionary.MergeTranslations([
            new CultureDictionaryRecord("{0} item for {1}", s_context, ["{0} элемент для {1}", "{0} элемента для {1}", "{0} элементов для {1}"]),
        ]);
        localizationManager.Setup(m => m.GetDictionary(It.Is<CultureInfo>(c => c.Name == "ru"))).Returns(dictionary);

        var factory = new PortableObjectStringLocalizerFactory(
            localizationManager.Object,
            Options.Create(new RequestLocalizationOptions()),
            NullLogger<PortableObjectStringLocalizerFactory>.Instance);

        using var cultureScope = CultureScope.Create("ru");
        var source = LocalizationSource.Create<StringLocalizerFactoryPluralExtensionsTests>("{0} item for {1}");

        var result = factory.Plural(5, source, "{0} items for {1}", "Mike");

        Assert.Equal("5 элементов для Mike", result.Value);
    }

    [Fact]
    public void Plural_And_Localize_OnSameSource_BothResolveTheSameLocalizerType()
    {
        var source = LocalizationSource.Create<StringLocalizerFactoryPluralExtensionsTests>("item");
        var localizer = new Mock<IStringLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["item"]).Returns(new LocalizedString("item", "translated-item"));
        localizer.Setup(l => l["item", It.IsAny<PluralizationArgument>()]).Returns(new LocalizedString("item", "2 translated-items"));
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var singular = factory.Object.Localize(source);
        var plural = factory.Object.Plural(2, source, "items");

        Assert.Equal("translated-item", singular.Value);
        Assert.Equal("2 translated-items", plural.Value);
        factory.Verify(f => f.Create(source.Type), Times.Exactly(2));
    }
}
