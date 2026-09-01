using OrchardCore.Media.Core;

namespace OrchardCore.Tests.Modules.OrchardCore.Media;

public class MediaCachePathEscaperTests
{
    [Theory]
    [InlineData("")]
    [InlineData("images/photo.png")]
    [InlineData("sub/dir/file.jpg")]
    [InlineData("with space/foo bar.png")]
    [InlineData("bàr/日本語.jpg")]
    public void Escape_SafePath_ReturnsSameInstance(string path)
        => Assert.Same(path, MediaCachePathEscaper.Escape(path));

    [Theory]
    [InlineData("test:asdf/pic.png", "test%3Aasdf/pic.png")] // Issue #17644.
    [InlineData("a<b>c", "a%3Cb%3Ec")]
    [InlineData("what?.png", "what%3F.png")]
    [InlineData("star*quote\"pipe|.txt", "star%2Aquote%22pipe%7C.txt")]
    [InlineData("100%.png", "100%25.png")]
    [InlineData("back\\slash.png", "back%5Cslash.png")]
    [InlineData("tab\tname.png", "tab%09name.png")]
    [InlineData("trailing./file.png", "trailing%2E/file.png")]
    [InlineData("trailing /file.png", "trailing%20/file.png")]
    [InlineData("dots.../x", "dots%2E%2E%2E/x")]
    [InlineData("CON", "%43ON")]
    [InlineData("con.txt", "%63on.txt")]
    [InlineData("lpt1.config.json", "%6Cpt1.config.json")]
    [InlineData("CONSOLE.txt", "CONSOLE.txt")]
    [InlineData("COM10.txt", "COM10.txt")]
    public void Escape_NtfsInvalidName_EscapesOffendingCharacters(string path, string expected)
        => Assert.Equal(expected, MediaCachePathEscaper.Escape(path));

    [Theory]
    [InlineData("test:asdf/pic.png")]
    [InlineData("a<b>c")]
    [InlineData("what?.png")]
    [InlineData("star*quote\"pipe|.txt")]
    [InlineData("100%/50%off.png")]
    [InlineData("%25already/escaped%3A.png")]
    [InlineData("trailing./file.png")]
    [InlineData("dots.../x")]
    [InlineData("CON/aux.txt")]
    [InlineData("images/photo.png")]
    public void Unescape_EscapedPath_RoundTrips(string path)
        => Assert.Equal(path, MediaCachePathEscaper.Unescape(MediaCachePathEscaper.Escape(path)));

    [Theory]
    [InlineData("50%off.png", "50%off.png")] // '%' not followed by two hex digits stays literal.
    [InlineData("a%3a.png", "a:.png")] // Lowercase hex is accepted.
    [InlineData("%", "%")]
    [InlineData("%2", "%2")]
    [InlineData("test%3Aasdf/pic.png", "test:asdf/pic.png")]
    public void Unescape_Input_ReturnsExpected(string path, string expected)
        => Assert.Equal(expected, MediaCachePathEscaper.Unescape(path));
}
