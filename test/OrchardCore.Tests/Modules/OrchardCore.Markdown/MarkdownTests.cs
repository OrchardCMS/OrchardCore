using Markdig;
using OrchardCore.Markdown.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Markdown
{
    public class MarkdownTests
    {
        [Fact]
        public void ShouldConfigureMarkdownPipeline()
        {
            var services = new ServiceCollection();
            services.AddOptions<MarkdownPipelineOptions>();
            services.ConfigureMarkdownPipeline((pipeline) => pipeline.DisableHtml());

            services.AddScoped<IMarkdownService, DefaultMarkdownService>();

            var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

            var markdown = @"<h1>foo</h1>";
            var html = markdownService.ToHtml(markdown);
            html = html.Replace("\n", "");
            Assert.Equal(@"<p>&lt;h1&gt;foo&lt;/h1&gt;</p>", html);
        }

        [Fact]
        public void ShouldReconfigureMarkdownPipeline()
        {
            // Setup. With defaults.
            var services = new ServiceCollection();
            services.AddOptions<MarkdownPipelineOptions>();
            services.ConfigureMarkdownPipeline((pipeline) => pipeline.DisableHtml());

            services.AddScoped<IMarkdownService, DefaultMarkdownService>();

            // Act. Clear configuration
            services.Configure<MarkdownPipelineOptions>(o =>
            {
                o.Configure.Clear();
            });

            // Test.
            var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

            var markdown = @"<h1>foo</h1>";
            var html = markdownService.ToHtml(markdown);
            html = html.Replace("\n", "");
            Assert.Equal(@"<h1>foo</h1>", html);
        }
    }
}
