using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using OrchardCore.Security.Middlewares;
using OrchardCore.Security.Options;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class ContentTypeOptionsMiddlewareTests
    {
        [Fact]
        public async Task AddContentTypeOptionsHeader()
        {
            // Arrange
            var options = MicrosoftOptions.Create(new ContentTypeOptionsOptions());
            var middleware = new ContentTypeOptionsMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XContentTypeOptions));
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
