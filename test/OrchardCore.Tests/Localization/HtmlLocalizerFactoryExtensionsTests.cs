using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.DisplayManagement.Html;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class HtmlLocalizerFactoryExtensionsTests
{
    [Fact]
    public void Localize_WithNullFactory_Throws()
    {
        Assert.Throws<ArgumentNullException>("factory", () => ((IHtmlLocalizerFactory)null).Localize(LocalizationSource.Create("Hello")));
    }

    [Fact]
    public void Localize_WithNullSource_ReturnsNull()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);

        Assert.Null(factory.Object.Localize(null));
        factory.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Hello {0}")]
    [InlineData("<strong>Hello</strong>")]
    public void Localize_WithoutContext_KeepsSourceValue(string value)
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        var source = LocalizationSource.Create(value);

        var localized = factory.Object.Localize(source);

        Assert.Equal(value, localized.Name);
        Assert.Equal(value, localized.Value);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithoutContextOrArguments_RendersHtml()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);

        var localized = factory.Object.Localize(LocalizationSource.Create("<strong>Hello</strong>"));

        Assert.Equal("<strong>Hello</strong>", Render(localized));
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithMissingFormatArgument_UsesStandardHtmlFormatting()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);

        var localized = factory.Object.Localize(LocalizationSource.Create("Hello {0}"));

        Assert.Throws<FormatException>(() => Render(localized));
    }

    [Fact]
    public void Localize_WithoutContextAndWithArguments_EncodesArgumentsOnly()
    {
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        var source = LocalizationSource.Create("<strong>Hello {0}</strong>");

        var localized = factory.Object.Localize(source, "<Mike>");

        Assert.Equal("<strong>Hello &lt;Mike&gt;</strong>", Render(localized));
        Assert.Equal("<strong>Hello {0}</strong>", source.Value);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithContext_UsesSourceType()
    {
        var source = LocalizationSource.Create<HtmlLocalizerFactoryExtensionsTests>("Hello");
        var localizer = new Mock<IHtmlLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["Hello"]).Returns(new LocalizedHtmlString("Hello", "<strong>Bonjour</strong>"));
        var factory = new Mock<IHtmlLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var localized = factory.Object.Localize(source);

        Assert.Equal("<strong>Bonjour</strong>", Render(localized));
        factory.Verify(f => f.Create(source.Type), Times.Once);
        localizer.Verify(l => l["Hello"], Times.Once);
        factory.VerifyNoOtherCalls();
        localizer.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_SameSourceWithDifferentFactories_DefersEncodingToRendering()
    {
        var source = LocalizationSource.Create<HtmlLocalizerFactoryExtensionsTests>("Hello {0}");
        object[] arguments = ["<Mike>"];
        var localizer = new Mock<IStringLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l[source.Value]).Returns(new LocalizedString(source.Value, "<strong>Bonjour {0}</strong>"));
        localizer.Setup(l => l[source.Value, arguments]).Returns(new LocalizedString(source.Value, "<strong>Bonjour <Mike></strong>"));
        var stringFactory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        stringFactory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);
        var htmlFactory = new HtmlLocalizerFactory(stringFactory.Object);

        var text = stringFactory.Object.Localize(source, arguments);
        var html = htmlFactory.Localize(source, arguments);

        Assert.Equal("&lt;strong&gt;Bonjour &lt;Mike&gt;&lt;/strong&gt;", Render(new HtmlContentString(text.Value)));
        Assert.Equal("<strong>Bonjour &lt;Mike&gt;</strong>", Render(html));
        Assert.Equal("Hello {0}", source.Value);
    }

    private static string Render(IHtmlContent content)
    {
        using var writer = new StringWriter();
        content.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
