using OrchardCore.Infrastructure.Html;
using OrchardCore.Markdown.Services;
using OrchardCore.Shortcodes.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Markdown;

public class MarkdownTests
{
    [Fact]
    public void ShouldConfigureMarkdownPipeline()
    {
        var services = CreateServiceCollection();
        var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

        var markdown = @"<h1>foo</h1>";
        var html = markdownService.ToHtml(markdown);
        html = html.ReplaceLineEndings(string.Empty);
        Assert.Equal(@"<p>&lt;h1&gt;foo&lt;/h1&gt;</p>", html);
    }

    [Fact]
    public void ShouldReconfigureMarkdownPipeline()
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
    public async Task ShouldConvertMarkdownToHtmlAsync()
    {
        // Setup.
        var services = CreateServiceCollection();
        services.AddScoped<IOrchardHelper>(provider => new MockOrchardHelper(provider));
        services.AddScoped<IShortcodeService, ShortcodeService>();
        services.AddScoped<IHtmlSanitizerService, HtmlSanitizerService>();
        
        // Act.
        var orchard = services.BuildServiceProvider().GetService<IOrchardHelper>();
        
        await using var stringWriter = new StringWriter();
        (await orchard.MarkdownToHtmlAsync("This _is_ a ==test== markdown.")).WriteTo(stringWriter, HtmlEncoder.Default);
        var html = stringWriter.ToString();

        // Test.
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
}
