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

    private static ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();

        services.AddOptions<MarkdownPipelineOptions>();
        services.ConfigureMarkdownPipeline(pipeline => pipeline.Configure("nohtml+advanced"));
        services.AddScoped<IMarkdownService, DefaultMarkdownService>();

        return services;
    }
}
