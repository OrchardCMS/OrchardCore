namespace OrchardCore.ContentManagement.Utilities.Tests;

public class StringExtensionsTests
{
    private const string DefaultEllipsis = "\u00A0\u2026";
    private const string CustomEllipsis = " >>>";

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Orchard", "Orchard")]
    [InlineData("OrchardCore", "Orchard Core")]
    [InlineData("orchardCore", "orchard Core")]
    public void CamelFriendly_ShouldReturnCamelCase(string value, string expected)
    {
        // Arrange & Act
        var result = StringExtensions.CamelFriendly(value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("Orchard Core", 70, "Orchard Core")]
    [InlineData("Orchard Core", 4, $"Orch{DefaultEllipsis}")]
    [InlineData("Orchard Core", 7, $"Orchard{DefaultEllipsis}")]
    public void Ellipsize_ShouldTrimString(string text, int characterCount, string expected)
    {
        // Arrange & Act
        var result = StringExtensions.Ellipsize(text, characterCount);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, 10, DefaultEllipsis, false, "")]
    [InlineData("", 10, DefaultEllipsis, false, "")]
    [InlineData("Orchard Core", 70, DefaultEllipsis, false, "Orchard Core")]
    [InlineData("Orchard Core", 4, DefaultEllipsis, false, $"Orch{DefaultEllipsis}")]
    [InlineData(null, 10, DefaultEllipsis, true, "")]
    [InlineData("", 10, DefaultEllipsis, true, "")]
    [InlineData("Orchard Core", 70, DefaultEllipsis, true, "Orchard Core")]
    [InlineData("Orchard Core", 7, DefaultEllipsis, true, DefaultEllipsis)]
    [InlineData("Orchard Core", 7, CustomEllipsis, true, CustomEllipsis)]
    [InlineData("Orchard Core", 10, CustomEllipsis, true, $"Orchard{CustomEllipsis}")]
    public void Ellipsize_ShouldTrimString_WithCustomEllipsisString(string text, int characterCount, string ellipsis, bool wordBoundary, string expected)
    {
        // Arrange & Act
        var result = StringExtensions.Ellipsize(text, characterCount, ellipsis, wordBoundary);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Welcome to <h1>Orchard Core</h1>", "Welcome to Orchard Core")]
    [InlineData("Welcome to Orchard Core", "Welcome to Orchard Core")]
    public void ShouldRemoveTagsFromString(string text, string expected)
    {
        // Arrange & Act
        var result = StringExtensions.RemoveTags(text);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, false, "")]
    [InlineData("", false, "")]
    [InlineData("Welcome to <h1>Orchard Core</h1>", false, "Welcome to Orchard Core")]
    [InlineData("Welcome to &lt;h1&gt;Orchard Core&lt;/h1&gt;", false, "Welcome to &lt;h1&gt;Orchard Core&lt;/h1&gt;")]
    [InlineData("Welcome to Orchard Core", false, "Welcome to Orchard Core")]
    [InlineData(null, true, "")]
    [InlineData("", true, "")]
    [InlineData("Welcome to <h1>Orchard Core</h1>", true, "Welcome to Orchard Core")]
    [InlineData("Welcome to &lt;h1&gt;Orchard Core&lt;/h1&gt;", true, "Welcome to <h1>Orchard Core</h1>")]
    [InlineData("Welcome to Orchard Core", true, "Welcome to Orchard Core")]
    public void ShouldRemoveTagsFromString_WithHtmlDecode(string text, bool htmlEncode, string expected)
    {
        // Arrange & Act
        var result = StringExtensions.RemoveTags(text, htmlEncode);

        // Assert
        Assert.Equal(expected, result);
    }
}
