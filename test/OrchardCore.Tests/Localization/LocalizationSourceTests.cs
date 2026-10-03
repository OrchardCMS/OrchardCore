using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class LocalizationSourceTests
{
    [Theory]
    [InlineData("", typeof(LocalizationSourceTests))]
    [InlineData("Hello", typeof(LocalizationSourceTests))]
    [InlineData("Hello {0}", typeof(LocalizationSourceTests))]
    [InlineData("<strong>Hello</strong>", typeof(LocalizationSourceTests))]
    public void Create_WithType_KeepsUntranslatedValueAndType(string value, Type type)
    {
        var source = LocalizationSource.Create(value, type);
        var genericSource = LocalizationSource.Create<LocalizationSourceTests>(value);

        Assert.Equal(value, source.Value);
        Assert.Equal(typeof(LocalizationSourceTests), source.Type);
        Assert.Equal(source, genericSource);
        Assert.NotSame(source, genericSource);
    }

    [Fact]
    public void Create_WithoutType_HasNoLocalizationContext()
    {
        var source = LocalizationSource.Create("Hello");

        Assert.Equal("Hello", source.Value);
        Assert.Null(source.Type);
        Assert.Equal(source, LocalizationSource.Create("Hello", type: null));
    }

    [Fact]
    public void Create_WithStaticType_KeepsLocalizationContext()
    {
        var source = LocalizationSource.Create("Hello", typeof(Math));

        Assert.Equal("Hello", source.Value);
        Assert.Equal(typeof(Math), source.Type);
    }

    [Theory]
    [InlineData(typeof(LocalizationSourceTests))]
    [InlineData(typeof(Math))]
    public void Create_WithNullValue_Throws(Type type)
    {
        Assert.Throws<ArgumentNullException>("value", () => LocalizationSource.Create(null));
        Assert.Throws<ArgumentNullException>("value", () => LocalizationSource.Create(null, type));
        Assert.Throws<ArgumentNullException>("value", () => LocalizationSource.Create<LocalizationSourceTests>(null));
    }

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
