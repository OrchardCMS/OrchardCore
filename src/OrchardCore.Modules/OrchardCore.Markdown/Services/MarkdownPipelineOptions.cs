using System;
using Markdig;

namespace OrchardCore.Markdown.Services
{
    public class MarkdownPipelineOptions
    {
        public Action<MarkdownPipelineBuilder> Configure { get; set; }
    }
}
