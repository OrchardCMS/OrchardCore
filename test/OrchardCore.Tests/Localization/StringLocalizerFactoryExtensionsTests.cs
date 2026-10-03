using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class StringLocalizerFactoryExtensionsTests
{
    [Fact]
    public void Localize_WithNullFactory_Throws()
    {
        Assert.Throws<ArgumentNullException>("factory", () => ((IStringLocalizerFactory)null).Localize(new LocalizationSource("Hello")));
    }

    [Fact]
    public void Localize_WithNullSource_ReturnsNull()
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);

        Assert.Null(factory.Object.Localize(null));
        factory.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Hello")]
    [InlineData("Hello {0}")]
    [InlineData("<strong>Hello</strong>")]
    public void Localize_WithoutContext_ReturnsSourceValue(string value)
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        var source = new LocalizationSource(value);

        var localized = factory.Object.Localize(source);

        Assert.Equal(value, localized.Name);
        Assert.Equal(value, localized.Value);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithoutContextAndWithArguments_FormatsAtDisplayTime()
    {
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        var source = new LocalizationSource("Hello {0}");

        var localized = factory.Object.Localize(source, "Mike");

        Assert.Equal("Hello {0}", localized.Name);
        Assert.Equal("Hello Mike", localized.Value);
        Assert.Equal("Hello {0}", source.Value);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithContext_UsesSourceType()
    {
        var source = new LocalizationSource("Hello", typeof(StringLocalizerFactoryExtensionsTests));
        var localizer = new Mock<IStringLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l["Hello"]).Returns(new LocalizedString("Hello", "Bonjour"));
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var localized = factory.Object.Localize(source);

        Assert.Equal("Bonjour", localized.Value);
        factory.Verify(f => f.Create(source.Type), Times.Once);
        localizer.Verify(l => l["Hello"], Times.Once);
        factory.VerifyNoOtherCalls();
        localizer.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithContextAndArguments_FormatsTranslation()
    {
        var source = new LocalizationSource("Hello {0}", typeof(StringLocalizerFactoryExtensionsTests));
        object[] arguments = ["Mike"];
        var localizer = new Mock<IStringLocalizer>(MockBehavior.Strict);
        localizer.Setup(l => l[source.Value, arguments]).Returns(new LocalizedString(source.Value, "Bonjour Mike"));
        var factory = new Mock<IStringLocalizerFactory>(MockBehavior.Strict);
        factory.Setup(f => f.Create(source.Type)).Returns(localizer.Object);

        var localized = factory.Object.Localize(source, arguments);

        Assert.Equal("Bonjour Mike", localized.Value);
        Assert.Equal("Hello {0}", source.Value);
        localizer.Verify(l => l[source.Value, arguments], Times.Once);
    }
}
