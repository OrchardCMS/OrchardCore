namespace OrchardCore.Tests.Localization;

public class LocalizedHtmlStringExtensionsTests
{
    [Theory]
    [InlineData("Hello")]
    [InlineData("<strong>Hello</strong>")]
    [InlineData("")]
    public void Create_WithName_UsesNameAsValue(string name)
    {
        // Act
        var localizedHtmlString = LocalizedHtmlString.Create(name);

        // Assert
        Assert.Equal(name, localizedHtmlString.Name);
        Assert.Equal(name, localizedHtmlString.Value);
        Assert.False(localizedHtmlString.IsResourceNotFound);
    }

    [Fact]
    public void Create_WithHtmlName_WritesValueWithoutEncoding()
    {
        // Arrange
        var localizedHtmlString = LocalizedHtmlString.Create("<strong>Hello</strong>");

        using var writer = new StringWriter();

        // Act
        localizedHtmlString.WriteTo(writer, HtmlEncoder.Default);

        // Assert
        Assert.Equal("<strong>Hello</strong>", writer.ToString());
    }

    [Fact]
    public void Create_WithNullName_Throws()
    {
        Assert.Throws<ArgumentNullException>("name", () => LocalizedHtmlString.Create(null));
    }

    [Fact]
    public void Create_CalledOnExtensionClass_UsesNameAsValue()
    {
        // Act
        var localizedHtmlString = LocalizedHtmlStringExtensions.Create("Hello");

        // Assert
        Assert.Equal("Hello", localizedHtmlString.Name);
        Assert.Equal("Hello", localizedHtmlString.Value);
    }
}
