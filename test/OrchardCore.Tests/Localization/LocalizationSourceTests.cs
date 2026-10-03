using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationSourceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("Hello")]
    [InlineData("Hello {0}")]
    [InlineData("<strong>Hello</strong>")]
    public void Constructor_WithType_KeepsUntranslatedValueAndType(string value)
    {
        var source = new LocalizationSource(value, typeof(LocalizationSourceTests));

        Assert.Equal(value, source.Value);
        Assert.Equal(typeof(LocalizationSourceTests), source.Type);
    }

    [Fact]
    public void Constructor_WithoutType_HasNoLocalizationContext()
    {
        var source = new LocalizationSource("Hello");

        Assert.Equal("Hello", source.Value);
        Assert.Null(source.Type);
    }

    [Fact]
    public void Constructor_WithNullValue_Throws()
    {
        Assert.Throws<ArgumentNullException>("value", () => new LocalizationSource(null, typeof(LocalizationSourceTests)));
    }

    [Fact]
    public void Equals_SameValueAndType_HasValueEquality()
    {
        var source = new LocalizationSource("Hello", typeof(LocalizationSourceTests));

        Assert.Equal(source, new LocalizationSource("Hello", typeof(LocalizationSourceTests)));
        Assert.NotEqual(source, new LocalizationSource("Hello"));
        Assert.NotEqual(source, new LocalizationSource("Goodbye", typeof(LocalizationSourceTests)));
    }
}
