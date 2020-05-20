using System;
using Ganss.XSS;

namespace OrchardCore.Infrastructure.Html
{
    public class HtmlSanitizerOptions
    {
        public Action<Ganss.XSS.HtmlSanitizer> Configure { get; set; }
    }
}
