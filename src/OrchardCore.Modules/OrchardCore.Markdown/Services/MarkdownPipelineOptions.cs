using System;
using System.Collections.Generic;
using Markdig;

namespace OrchardCore.Markdown.Services
{
    public class MarkdownPipelineOptions
    {
        public IList<Action<MarkdownPipelineBuilder>> Configure { get; } = new List<Action<MarkdownPipelineBuilder>>();
    }
}
