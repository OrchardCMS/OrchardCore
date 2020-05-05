using Ganss.XSS;
using Microsoft.Extensions.Options;

namespace OrchardCore.Infrastructure.Script
{
    public class HtmlScriptSanitizer : IHtmlScriptSanitizer
    {
        private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public HtmlScriptSanitizer(IOptions<HtmlScriptSanitizerOptions> options)
        {
            if (options.Value.Configure != null)
            {
                options.Value.Configure.Invoke(_sanitizer);
            }
        }

        public string Sanitize(string html)
        {
            return _sanitizer.Sanitize(html);
        }
    }
}
