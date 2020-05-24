using Ganss.XSS;
using Microsoft.Extensions.Options;

namespace OrchardCore.Infrastructure.Html
{
    public class HtmlSanitizerService : IHtmlSanitizerService
    {
        private readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public HtmlSanitizerService(IOptions<HtmlSanitizerOptions> options)
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
