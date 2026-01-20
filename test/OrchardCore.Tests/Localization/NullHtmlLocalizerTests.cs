using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization;

public class HtmlStringLocalizerTests
{
    [Theory]
    [InlineData("there is one", 1, "there is one", "there are more")]
    [InlineData("there are more", 2, "there is one", "there are more")]
    [InlineData("there is one (1)", 1, "there is one ({0})", "there are more ({0})")]
    [InlineData("there are more (2)", 2, "there is one ({0})", "there are more ({0})")]
    [InlineData("there is one (1) thing", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", "thing")]
    [InlineData("there are more (2) thing", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", "thing")]
    [InlineData("there is one (1) &lt;br/&gt;", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", "<br/>")]
    [InlineData("there are more (2) &lt;br/&gt;", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", "<br/>")]
    [InlineData("1 minute ago", 1, "{0} minute ago", "{0} minutes ago")]
    [InlineData("20 minutes ago", 20, "{0} minute ago", "{0} minutes ago")]
    public void HtmlNullLocalizerSupportsPlural(string expected, int count, string singular, string plural, params object[] arguments)
    {
        var localizer = NullHtmlLocalizer.Instance;

        using var writer = new StringWriter();
        localizer.Plural(count, singular, plural, arguments).WriteTo(writer, HtmlEncoder.Default);
        Assert.Equal(expected, writer.ToString());
    }
}
