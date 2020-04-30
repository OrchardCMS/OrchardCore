using OrchardCore.Infrastructure.Script;
using Xunit;

namespace OrchardCore.Tests.Script
{
    public class HtmlScriptSanitizerTests
    {
        private static HtmlScriptSanitizer _sanitizer = new HtmlScriptSanitizer();

        [Theory]
        [InlineData("<script>alert('xss')</script><div onload=\"alert('xss')\">Test<img src=\"test.gif\" style=\"background-image: url(javascript:alert('xss')); margin: 10px\"></div>", "<div>Test<img src=\"test.gif\" style=\"margin: 10px\"></div>")]
        [InlineData("<IMG SRC=javascript:alert(\"XSS\")>", @"<img>")]
        [InlineData("<a href=\"javascript: alert('xss')\">Click me</a>", @"<a>Click me</a>")]
        public void ShouldSanitizeHTML(string html, string sanitized)
        {
            // Setup
            var output = _sanitizer.Sanitize(html);

            // Test
            Assert.Equal(output, sanitized);
        }


        [Theory]
        [InlineData("[Click me](javascript:alert('xss'))", "[Click me](javascript:alert('xss'))")]
        public void DoesNotSanitizeMarkdown(string markdown, string sanitized)
        {
            // Setup
            var output = _sanitizer.Sanitize(markdown);

            // Test
            Assert.Equal(output, sanitized);
        }
    }
}
