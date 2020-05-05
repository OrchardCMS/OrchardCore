using System;
using Ganss.XSS;

namespace OrchardCore.Infrastructure.Script
{
    public class HtmlScriptSanitizerOptions
    {
        public Action<HtmlSanitizer> Configure { get; set; }
    }
}
