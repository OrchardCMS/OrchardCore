using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentManagement.Script;
using Xunit;

namespace OrchardCore.Tests.Script
{
    public class HtmlScriptSanitizerTests
    {
        [Fact]
        public void ShouldSanitizeHTML()
        {
            // Setup
            var sanitizer = new HtmlScriptSanitizer();

            var html = @"<script>alert('xss')</script><div onload=""alert('xss')"" style=""background-color: test"">Test<img src=""test.gif"" style=""background-image: url(javascript:alert('xss')); margin: 10px""></div>";

            // Act
            var sanitized = sanitizer.Sanitize(html);

            // Test
            Assert.Equal(@"<div>Test<img src=""test.gif"" style=""margin: 10px""></div>", sanitized); //style=""background-color: test""
        }
    }
}
