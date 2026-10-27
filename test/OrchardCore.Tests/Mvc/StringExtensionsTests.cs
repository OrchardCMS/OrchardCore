using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Tests.Mvc;

public class StringExtensionsTests
{
    private const string DefaultEllipsis = " …";

    [Theory]
    [InlineData(null, 10, "")]
    [InlineData("", 10, "")]
    [InlineData("Orchard Core", 70, "Orchard Core")]
    [InlineData("Orchard Core", 4, $"Orch{DefaultEllipsis}")]
    [InlineData("Orchard Core", 7, $"Orchard{DefaultEllipsis}")]
    [InlineData("Orchard Core Framework", 12, $"Orchard Core{DefaultEllipsis}")]
    [InlineData("Orchard Core Framework", 13, $"Orchard Core{DefaultEllipsis}")]
    [InlineData("Orchard Core Framework", 14, $"Orchard Core{DefaultEllipsis}")]
    public void Ellipsize_Default_TrimsString(string text, int characterCount, string expected)
    {
        var result = text.Ellipsize(characterCount);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Orchard Core", 4, DefaultEllipsis)]
    [InlineData("Orchard Core", 7, $"Orchard{DefaultEllipsis}")]
    [InlineData("Orchard Core", 10, $"Orchard{DefaultEllipsis}")]
    public void Ellipsize_WordBoundary_KeepsWholeWords(string text, int characterCount, string expected)
    {
        var result = text.Ellipsize(characterCount, DefaultEllipsis, wordBoundary: true);

        Assert.Equal(expected, result);
    }
}
