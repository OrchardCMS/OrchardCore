using Markdig;

namespace OrchardCore.Markdown.Services;

public class MarkdownPipelineOptions
{
    public List<Action<MarkdownPipelineBuilder>> Configure { get; } = [];
}
