namespace OrchardCore.Tests.Localization;

public class StringLocalizerFactoryExtensionsTests
{
    [Fact]
    public void Create_WithResourceSource_SetsContext()
    {
        // Act
        var localizedString = LocalizedString.Create("Hello", typeof(StringLocalizerFactoryExtensionsTests));

        // Assert
        Assert.Equal("Hello", localizedString.Name);
        Assert.Equal("Hello", localizedString.Value);
        Assert.False(localizedString.ResourceNotFound);
        Assert.Equal(typeof(StringLocalizerFactoryExtensionsTests).FullName, localizedString.SearchedLocation);
    }

    [Fact]
    public void Create_WithNullResourceSource_Throws()
    {
        Assert.Throws<ArgumentNullException>("resourceSource", () => LocalizedString.Create("Hello", (Type)null));
    }

    [Fact]
    public void Localize_WithNullValue_ReturnsNull()
    {
        // Arrange
        var factory = new Mock<IStringLocalizerFactory>();

        // Act
        var localized = factory.Object.Localize(null);

        // Assert
        Assert.Null(localized);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithoutContext_ReturnsSameValue()
    {
        // Arrange
        var factory = new Mock<IStringLocalizerFactory>();
        var value = LocalizedString.Create("Hello");

        // Act
        var localized = factory.Object.Localize(value);

        // Assert
        Assert.Same(value, localized);
        factory.VerifyNoOtherCalls();
    }

    [Fact]
    public void Localize_WithContext_UsesLocalizerForContext()
    {
        // Arrange
        var context = typeof(StringLocalizerFactoryExtensionsTests).FullName;
        var localizer = new Mock<IStringLocalizer>();
        localizer.Setup(l => l["Hello"]).Returns(new LocalizedString("Hello", "Bonjour"));

        var factory = new Mock<IStringLocalizerFactory>();
        factory.Setup(f => f.Create(context, string.Empty)).Returns(localizer.Object);

        // Act
        var localized = factory.Object.Localize(LocalizedString.Create("Hello", typeof(StringLocalizerFactoryExtensionsTests)));

        // Assert
        Assert.Equal("Bonjour", localized.Value);
    }
}
