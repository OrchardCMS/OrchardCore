using Markdig;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Markdown.Services;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Markdown
{
    public class MarkdownTests
    {
        [Fact]
        public void ShouldConfigureMarkdownPipeline()
        {
            var services = new ServiceCollection();
            services.Configure<MarkdownPipelineOptions>(o =>
            {
                o.Configure = (pipeline) =>
                {
                    pipeline.DisableHtml();
                };
            });

            services.AddScoped<IMarkdownService, DefaultMarkdownService>();

            var markdownService = services.BuildServiceProvider().GetService<IMarkdownService>();

            var markdown = @"<h1>foo</h1>";
            var html = markdownService.ToHtml(markdown);
            html = html.Replace("\n", "");
            Assert.Equal(@"<p>&lt;h1&gt;foo&lt;/h1&gt;</p>", html);
        }
    }
}
