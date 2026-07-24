using OrchardCore.Infrastructure.Html;

namespace OrchardCore.Tests.Html;

public class HtmlSanitizerTests
{
    private static readonly HtmlSanitizerService _sanitizer = new(Options.Create(new HtmlSanitizerOptions()));

    [Theory]
    [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
    [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
    [InlineData("<a href=\"javascript: alert('xss')\">Click me</a>", @"<a>Click me</a>")]
    [InlineData("<a href=\"[locale 'en']javascript: alert('xss')[/locale]\">Click me</a>", @"<a>Click me</a>")]
    public void Sanitize_HTML_Succeeds(string html, string sanitized)
    {
        // Setup
        var output = _sanitizer.Sanitize(html);

        // Test
        Assert.Equal(output, sanitized);
    }

    [Fact]
    public void Configure_Sanitizer_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddOptions<HtmlSanitizerOptions>();
        services.ConfigureHtmlSanitizer((sanitizer) =>
        {
            sanitizer.AllowedAttributes.Add("class");
        });

        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        var sanitizer = services.BuildServiceProvider().GetService<IHtmlSanitizerService>();

        var input = @"<a href=""bar"" class=""foo"">baz</a>";
        var sanitized = sanitizer.Sanitize(input);
        Assert.Equal(input, sanitized);
    }

    [Fact]
    public void Reconfigure_Sanitizer_Succeeds()
    {
        // Setup. With defaults.
        var services = new ServiceCollection();
        services.AddOptions<HtmlSanitizerOptions>();
        services.ConfigureHtmlSanitizer((sanitizer) =>
        {
            sanitizer.AllowedAttributes.Add("class");
        });

        // Act. Reconfigure to remove defaults.
        services.Configure<HtmlSanitizerOptions>(o =>
        {
            o.Configure.Clear();
        });

        // Test.
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();

        var sanitizer = services.BuildServiceProvider().GetService<IHtmlSanitizerService>();

        var input = @"<a href=""bar"" class=""foo"">baz</a>";
        var sanitized = sanitizer.Sanitize(input);
        Assert.Equal(@"<a href=""bar"">baz</a>", sanitized);
    }
}
