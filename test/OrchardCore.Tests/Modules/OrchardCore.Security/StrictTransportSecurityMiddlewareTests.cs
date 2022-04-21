using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class StrictTransportSecurityMiddlewareTests
    {
        [Theory]
        [InlineData(0, false, false, "max-age=0")]
        [InlineData(-1, false, false, "max-age=-1")]
        [InlineData(0, true, false, "max-age=0; includeSubDomains")]
        [InlineData(50000, false, true, "max-age=50000; preload")]
        [InlineData(0, true, true, "max-age=0; includeSubDomains; preload")]
        [InlineData(50000, true, true, "max-age=50000; includeSubDomains; preload")]
        public async Task AddStrictTransportSecurityHeader(int maxAge, bool includeSubDomains, bool preload, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new StrictTransportSecurityOptions
            {
                MaxAge = TimeSpan.FromSeconds(maxAge),
                IncludeSubDomains = includeSubDomains,
                Preload = preload
            });
            var middleware = new StrictTransportSecurityMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.StrictTransportSecurity));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
