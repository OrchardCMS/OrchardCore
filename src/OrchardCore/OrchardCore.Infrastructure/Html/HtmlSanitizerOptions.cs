using System;
using Ganss.XSS;

namespace OrchardCore.Infrastructure.Html
{
    public class HtmlSanitizerOptions
    {
        public Action<HtmlSanitizer> Configure { get; set; }
    }
}
