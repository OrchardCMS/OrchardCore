using Fluid;
using Fluid.Values;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Liquid;
using OrchardCore.Markdown.Filters;
using OrchardCore.Markdown.Services;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Markdown;

public class MarkdownTests
{
    [Fact]
    public void Configure_MarkdownPipeline_Succeeds()
    {
        var services = CreateServiceCollection();
        var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

        var markdown = @"<h1>foo</h1>";
        var html = markdownService.ToHtml(markdown);
        html = html.ReplaceLineEndings(string.Empty);
        Assert.Equal(@"<p>&lt;h1&gt;foo&lt;/h1&gt;</p>", html);
    }

    [Fact]
    public void Reconfigure_MarkdownPipeline_Succeeds()
    {
        // Setup. With defaults.
        var services = CreateServiceCollection();

        // Act. Clear configuration
        services.Configure<MarkdownPipelineOptions>(o =>
        {
            o.Configure.Clear();
        });

        // Test.
        var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

        var markdown = @"<h1>foo</h1>";
        var html = markdownService.ToHtml(markdown);
        html = html.ReplaceLineEndings(string.Empty);
        Assert.Equal(@"<h1>foo</h1>", html);
    }

    [Fact]
    public async Task Convert_MarkdownToHtmlAsync_Succeeds()
    {
        // Arrange
        var services = CreateServiceCollection();
        services.AddScoped<IOrchardHelper>(provider => new MockOrchardHelper(provider));
        services.AddScoped<IShortcodeService, ShortcodeService>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        
        // Act
        var orchard = services.BuildServiceProvider().GetService<IOrchardHelper>();
        
        await using var stringWriter = new StringWriter();
        (await orchard.MarkdownToHtmlAsync("This _is_ a ==test== markdown.")).WriteTo(stringWriter, HtmlEncoder.Default);
        var html = stringWriter.ToString();

        // Assert
        html = html.ReplaceLineEndings(string.Empty);
        Assert.Equal("<p>This <em>is</em> a <mark>test</mark> markdown.</p>", html);
    }

    private static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();

        services.AddOptions<MarkdownPipelineOptions>();
        services.ConfigureMarkdownPipeline(pipeline => pipeline.Configure("nohtml+advanced"));
        services.AddScoped<IMarkdownService, DefaultMarkdownService>();

        return services;
    }
    
    private sealed class MockOrchardHelper : IOrchardHelper
    {
        public HttpContext HttpContext { get; }

        public MockOrchardHelper(IServiceProvider provider)
        {
            HttpContext = new DefaultHttpContext();
            HttpContext.RequestServices = provider;
        }
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>", "<p>&lt;script&gt;alert('xss')&lt;/script&gt;</p>\n")]
    [InlineData("<img src=x onerror=alert(1)>", "<p>&lt;img src=x onerror=alert(1)&gt;</p>\n")]
    [InlineData("<a href=\"javascript:alert(1)\">click</a>", "<p>&lt;a href=&quot;javascript:alert(1)&quot;&gt;click&lt;/a&gt;</p>\n")]
    public void DefaultMarkdownPipeline_Default_EscapeHtmlTags(string maliciousMarkdown, string expected)
    {
        // The default 'nohtml' pipeline escapes all raw HTML, making it display as text.
        var services = CreateServiceCollection();
        var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

        var html = markdownService.ToHtml(maliciousMarkdown);

        Assert.Equal(expected, html);
        // Verify no actual HTML elements are produced (tags are entity-escaped).
        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("<img ", html);
        Assert.DoesNotContain("<a ", html);
    }

    [Theory]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("<a href=\"javascript:alert(1)\">click</a>")]
    [InlineData("<iframe src=\"evil.com\"></iframe>")]
    [InlineData("<object data=\"evil.swf\"></object>")]
    public async Task MarkdownifyFilter_NoHtmlIsDisabled_SanitizeXssPayloads(string maliciousMarkdown)
    {
        // Even when 'nohtml' is removed from the pipeline (allowing raw HTML),
        // the markdownify filter must still sanitize dangerous HTML.
        var services = new ServiceCollection();
        services.AddOptions<MarkdownPipelineOptions>();
        // Configure WITHOUT nohtml - allowing raw HTML through Markdig.
        services.ConfigureMarkdownPipeline(pipeline => pipeline.Configure("advanced"));
        services.AddScoped<IMarkdownService, DefaultMarkdownService>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddOptions<HtmlSanitizerOptions>();

        var sp = services.BuildServiceProvider();
        var markdownService = sp.GetRequiredService<IMarkdownService>();
        var htmlSanitizerService = sp.GetRequiredService<IHtmlSanitizerService>();

        var filter = new Markdownify(markdownService, htmlSanitizerService);
        var result = await filter.ProcessAsync(
            new StringValue(maliciousMarkdown),
            new FilterArguments(),
            null);

        var html = result.ToStringValue();

        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("onerror", html);
        Assert.DoesNotContain("javascript:", html);
        Assert.DoesNotContain("<iframe", html);
        Assert.DoesNotContain("<object", html);
    }

    [Fact]
    public async Task MarkdownifyFilter_Default_PreservesSafeMarkdown()
    {
        var services = new ServiceCollection();
        services.AddOptions<MarkdownPipelineOptions>();
        services.ConfigureMarkdownPipeline(pipeline => pipeline.Configure("nohtml+advanced"));
        services.AddScoped<IMarkdownService, DefaultMarkdownService>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        services.AddOptions<HtmlSanitizerOptions>();

        var sp = services.BuildServiceProvider();
        var markdownService = sp.GetRequiredService<IMarkdownService>();
        var htmlSanitizerService = sp.GetRequiredService<IHtmlSanitizerService>();

        var filter = new Markdownify(markdownService, htmlSanitizerService);
        var result = await filter.ProcessAsync(
            new StringValue("This is **bold** and _italic_ text."),
            new FilterArguments(),
            null);

        var html = result.ToStringValue();

        Assert.Contains("<strong>bold</strong>", html);
        Assert.Contains("<em>italic</em>", html);
    }
}
