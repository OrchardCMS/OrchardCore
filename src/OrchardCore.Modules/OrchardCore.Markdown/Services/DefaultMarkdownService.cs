using Markdig;
using Microsoft.Extensions.Options;

namespace OrchardCore.Markdown.Services
{
    public class DefaultMarkdownService : IMarkdownService
    {
        private readonly MarkdownPipeline _markdownPipeline;

        public DefaultMarkdownService(IOptions<MarkdownPipelineOptions> options)
        {
            var builder = new MarkdownPipelineBuilder();
            if (options.Value.Configure != null)
            {
                options.Value.Configure.Invoke(builder);
            }
            _markdownPipeline = builder.Build();
        }

        public string ToHtml(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown, _markdownPipeline);
        }
    }
}
