using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class ContentTypeOptionsMiddlewareTests
    {
        [Fact]
        public async Task AddContentTypeOptionsHeader()
        {
            // Arrange
            var middleware = new ContentTypeOptionsMiddleware(ContentTypeOptions.NoSniff, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XContentTypeOptions));
            Assert.Equal(ContentTypeOptions.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
