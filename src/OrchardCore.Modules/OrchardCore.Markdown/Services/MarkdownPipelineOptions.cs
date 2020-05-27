using System;
using System.Collections.Generic;
using Markdig;

namespace OrchardCore.Markdown.Services
{
    public class MarkdownPipelineOptions
    {
        public Action<MarkdownPipelineBuilder> Configure { get; set; }
        public IList<Action<MarkdownPipelineBuilder>> Configure2 { get; set; } = new List<Action<MarkdownPipelineBuilder>>();
    }
}
