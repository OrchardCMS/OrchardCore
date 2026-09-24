namespace OrchardCore.Tests.Localization;

public class LocalizedHtmlStringExtensionsTests
{
    [Theory]
    [InlineData("Hello")]
    [InlineData("<strong>Hello</strong>")]
    [InlineData("")]
    public void Create_WithValue_UsesValueAsName(string value)
    {
        // Act
        var localizedHtmlString = LocalizedHtmlString.Create(value);

        // Assert
        Assert.Equal(value, localizedHtmlString.Name);
        Assert.Equal(value, localizedHtmlString.Value);
        Assert.False(localizedHtmlString.IsResourceNotFound);
    }

    [Fact]
    public void Create_WithHtmlValue_WritesValueWithoutEncoding()
    {
        // Arrange
        var localizedHtmlString = LocalizedHtmlString.Create("<strong>Hello</strong>");

        // Act
        var html = Render(localizedHtmlString);

        // Assert
        Assert.Equal("<strong>Hello</strong>", html);
    }

    [Fact]
    public void Create_WithArguments_WritesEncodedArguments()
    {
        // Arrange
        var localizedHtmlString = LocalizedHtmlString.Create("<strong>Hello {0}</strong>", "<Mike>");

        // Act
        var html = Render(localizedHtmlString);

        // Assert
        Assert.Equal("<strong>Hello {0}</strong>", localizedHtmlString.Name);
        Assert.Equal("<strong>Hello {0}</strong>", localizedHtmlString.Value);
        Assert.Equal("<strong>Hello &lt;Mike&gt;</strong>", html);
    }

    [Fact]
    public void Create_WithNullValue_Throws()
    {
        Assert.Throws<ArgumentNullException>("value", () => LocalizedHtmlString.Create(null));
    }

    [Fact]
    public void Create_CalledOnExtensionClass_UsesValueAsName()
    {
        // Act
        var localizedHtmlString = LocalizedHtmlStringExtensions.Create("Hello");

        // Assert
        Assert.Equal("Hello", localizedHtmlString.Name);
        Assert.Equal("Hello", localizedHtmlString.Value);
    }

    private static string Render(LocalizedHtmlString localizedHtmlString)
    {
        using var writer = new StringWriter();
        localizedHtmlString.WriteTo(writer, HtmlEncoder.Default);

        return writer.ToString();
    }
}
