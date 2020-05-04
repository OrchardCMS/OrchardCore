using Ganss.XSS;
using Microsoft.Extensions.Options;

namespace OrchardCore.Infrastructure.Script
{
    public class HtmlScriptSanitizer : IHtmlScriptSanitizer
    {

        private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        // TODO Provide configuration options.

        public string Sanitize(string html)
        {
            return _sanitizer.Sanitize(html);
        }
    }
}
