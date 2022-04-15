using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class XFrameOptionsMiddlewareTests
    {
        [Fact]
        public async Task AddXFrameOptionsHeader()
        {
            // Arrange
            var middleware = new FrameOptionsMiddleware(FrameOptions.SameOrigin, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
            Assert.Equal(FrameOptions.SameOrigin, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
