using System;
using System.Collections.Generic;
using Ganss.XSS;

namespace OrchardCore.Infrastructure.Html
{
    public class HtmlSanitizerOptions
    {
        public IList<Action<HtmlSanitizer>> Configure { get; } = new List<Action<HtmlSanitizer>>();
    }
}
