using Microsoft.AspNetCore.Html;
using OrchardCore.Localization;
using OrchardCore.Localization.PortableObject;

namespace OrchardCore.Tests.Localization;

public class HtmlLocalizerFactoryPluralExtensionsTests
{
    private static readonly string s_context = typeof(HtmlLocalizerFactoryPluralExtensionsTests).FullName;

    [Fact]
    public void Plural_WithNullFactory_Throws()
    {
        Assert.Throws<ArgumentNullException>("factory", () => ((IHtmlLocalizerFactory)null).Plural(1, new LocalizationSource("item"), "items"));
    }

    [Fact]
    public void Plural_WithNullSource_ReturnsNull()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);

        Assert.Null(factory.Object.Plural(1, null, "items"));
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_WithNullPlural_Throws()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);

        Assert.Throws<ArgumentNullException>("plural", () => factory.Object.Plural(1, new LocalizationSource("item"), null));
        factory.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(1, "1 item")]
    [InlineData(0, "0 items")]
    [InlineData(2, "2 items")]
    public void Plural_WithoutContext_SelectsEnglishFormAndPrependsCount(int count, string expected)
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        var source = new LocalizationSource("{0} item");

        var result = factory.Object.Plural(count, source, "{0} items");

        Assert.Equal(expected, Render(result));
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_WithoutContextAndArguments_EncodesArgumentsAtRenderTime()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        var source = new LocalizationSource("{0} item for {1}");

        var result = factory.Object.Plural(3, source, "{0} items for {1}", "<Mike>");

        Assert.Equal("3 items for &lt;Mike&gt;", Render(result));
        Assert.Equal("{0} item for {1}", source.Value);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_WithContext_UsesSourceTypeAndDeferEncodingToRendering()
    {
        var source = new LocalizationSource("item", typeof(HtmlLocalizerFactoryPluralExtensionsTests));
        var localizer = new Mock<IHtmlLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["item", It.IsAny<PluralizationArgument>()])
            .Returns(new LocalizedHtmlString("item", "<strong>2 Bonjour</strong>"));
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var result = factory.Object.Plural(2, source, "items");

        Assert.Equal("<strong>2 Bonjour</strong>", Render(result));
        factory.Verify(f => f.Create(source.Type), Times.Once);
        localizer.Verify(l => l["item", It.IsAny<PluralizationArgument>()], Times.Once);
        localizer.VerifyNoOtherCalls();
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Plural_SourceImmutability_DoesNotChangeAfterUse()
    {
        var source = new LocalizationSource("{0} item", typeof(HtmlLocalizerFactoryPluralExtensionsTests));
        var localizer = new Mock<IHtmlLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["{0} item", It.IsAny<PluralizationArgument>()]).Returns(new LocalizedHtmlString("{0} item", "2 items"));
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        factory.Object.Plural(2, source, "{0} items");

        Assert.Equal("{0} item", source.Value);
        Assert.Equal(typeof(HtmlLocalizerFactoryPluralExtensionsTests), source.Type);
    }

    [Theory]
    [InlineData(1, "1 элемент")]
    [InlineData(2, "2 элемента")]
    [InlineData(5, "5 элементов")]
    [InlineData(0, "0 элементов")]
    public void Plural_WithContext_UsesActualFactoryAndFollowsCulturePluralRules(int count, string expected)
    {
        var localizationManager = new Mock<ILocalizationManager>();
        var dictionary = new CultureDictionary("ru", PluralizationRule.Russian);
        dictionary.MergeTranslations([
            new CultureDictionaryRecord("{0} item", s_context, ["{0} элемент", "{0} элемента", "{0} элементов"]),
        ]);
        localizationManager.Setup(m => m.GetDictionary(It.Is<CultureInfo>(c => c.Name == "ru"))).Returns(dictionary);

        var stringLocalizer = new PortableObjectStringLocalizer(
            s_context,
            localizationManager.Object,
            fallBackToParentCulture: true,
            NullLogger.Instance);
        var htmlLocalizer = new PortableObjectHtmlLocalizer(stringLocalizer);
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(typeof(HtmlLocalizerFactoryPluralExtensionsTests))).Returns(htmlLocalizer);

        using var cultureScope = CultureScope.Create("ru");
        var source = new LocalizationSource("{0} item", typeof(HtmlLocalizerFactoryPluralExtensionsTests));

        var result = factory.Object.Plural(count, source, "{0} items");

        Assert.Equal(expected, Render(result));
    }

    [Fact]
    public void Plural_And_Localize_OnSameSource_ProduceConsistentEncodingSemantics()
    {
        var source = new LocalizationSource("item", typeof(HtmlLocalizerFactoryPluralExtensionsTests));
        var localizer = new Mock<IHtmlLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["item"]).Returns(new LocalizedHtmlString("item", "<strong>translated</strong>"));
        localizer.Setup(l => l["item", It.IsAny<PluralizationArgument>()]).Returns(new LocalizedHtmlString("item", "<strong>2 translated</strong>"));
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var singular = factory.Object.Localize(source);
        var plural = factory.Object.Plural(2, source, "items");

        Assert.Equal("<strong>translated</strong>", Render(singular));
        Assert.Equal("<strong>2 translated</strong>", Render(plural));
        factory.Verify(f => f.Create(source.Type), Times.Exactly(2));
    }

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
