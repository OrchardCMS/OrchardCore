using Ganss.XSS;

namespace OrchardCore.Infrastructure.Script
{
    public class HtmlScriptSanitizer : IHtmlScriptSanitizer
    {
        public string Sanitize(string html)
        {
            var sanitizer = new HtmlSanitizer();

            return sanitizer.Sanitize(html);
        }
    }
}
