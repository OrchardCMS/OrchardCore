using OrchardCore.Infrastructure.Html;

namespace OrchardCore.Tests.Html;

public class HtmlSanitizerTests
{
    private static readonly HtmlSanitizerService s_sanitizer = new(Options.Create(new HtmlSanitizerOptions()));

    [Theory]
    [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
    [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
    [InlineData("<a href=\"javascript: alert('xss')\">Click me</a>", @"<a>Click me</a>")]
    [InlineData("<a href=\"[locale 'en']javascript: alert('xss')[/locale]\">Click me</a>", @"<a>Click me</a>")]
    public void Sanitize_HTML_Succeeds(string html, string sanitized)
    {
        // Setup
        var output = s_sanitizer.Sanitize(html);

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

    [Fact]
    public void Sanitize_SameFragmentTwice_ReusesSanitizedResult()
    {
        // Arrange
        var sanitizer = new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));
        var html = "<p onclick=\"alert('xss')\">Cached <script>alert('xss')</script></p>";

        // Act
        var first = sanitizer.Sanitize(html);
        var second = sanitizer.Sanitize(new string(html.AsSpan()));

        // Assert
        Assert.Equal("<p>Cached </p>", first);
        Assert.Same(first, second);
    }

    [Fact]
    public void Sanitize_FragmentTooLargeToBeCached_IsSanitized()
    {
        // Arrange
        var sanitizer = new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));
        var text = new string('a', 70_000);
        var html = $"<p>{text}<script>alert('xss')</script></p>";

        // Act
        var first = sanitizer.Sanitize(html);
        var second = sanitizer.Sanitize(html);

        // Assert
        Assert.Equal($"<p>{text}</p>", first);
        Assert.Equal(first, second);
        Assert.NotSame(first, second);
    }

    [Fact]
    public void Sanitize_SameFragmentWithDifferentConfigurations_DoesNotShareResults()
    {
        // Arrange
        var permissive = new HtmlSanitizerOptions();
        permissive.Configure.Add(sanitizer => sanitizer.AllowedTags.Add("blink"));

        var tenantA = new HtmlSanitizerService(Options.Create(permissive));
        var tenantB = new HtmlSanitizerService(Options.Create(new HtmlSanitizerOptions()));
        var html = "<blink>Cached per service</blink>";

        // Act
        var resultA = tenantA.Sanitize(html);
        var resultB = tenantB.Sanitize(html);

        // Assert
        Assert.Equal(html, resultA);
        Assert.Empty(resultB);
    }
}
