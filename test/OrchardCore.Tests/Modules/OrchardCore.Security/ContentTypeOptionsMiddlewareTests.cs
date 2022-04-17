using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class ContentTypeOptionsMiddlewareTests
    {
        [Theory]
        [InlineData(false, "nosniff")]
        [InlineData(true, "")]
        public async Task AddContentTypeOptionsHeader(bool allowSniffing, string expectedValue)
        {
            // Arrange
            var options = Options.Create(new ContentTypeOptionsOptions { AllowSniffing = allowSniffing });
            var middleware = new ContentTypeOptionsMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XContentTypeOptions));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
