using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class FrameOptionsMiddlewareTests
    {
        public static IEnumerable<object[]> FrameOptions =>
            new List<object[]>
            {
                new object[] { FrameOptionsValue.Deny, "DENY" },
                new object[] { FrameOptionsValue.SameOrigin, "SAMEORIGIN" }
            };

        [Theory]
        [MemberData(nameof(FrameOptions))]
        public async Task AddFrameOptionsHeader(FrameOptionsValue value, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new FrameOptionsOptions { Value = value });
            var middleware = new FrameOptionsMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
