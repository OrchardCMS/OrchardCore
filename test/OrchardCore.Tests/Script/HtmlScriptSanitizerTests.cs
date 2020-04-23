using OrchardCore.Infrastructure.Script;
using Xunit;

namespace OrchardCore.Tests.Script
{
    public class HtmlScriptSanitizerTests
    {
        [Theory]
        [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
        [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
        public void ShouldSanitizeHTML(string html, string sanitized)
        {
            // Setup
            var sanitizer = new HtmlScriptSanitizer();

            // Test
            Assert.Equal(sanitizer.Sanitize(html), sanitized);
        }
    }
}
