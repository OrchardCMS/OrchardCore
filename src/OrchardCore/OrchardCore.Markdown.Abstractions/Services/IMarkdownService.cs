using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Markdown.Services
{
    public interface IMarkdownService
    {
        string ToHtml(string markdown);
    }
}
