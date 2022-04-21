using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using OrchardCore.Security.Options;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class ReferrerPolicyMiddlewareTests
    {
        public static IEnumerable<object[]> Policies =>
            new List<object[]>
            {
                new object[] { ReferrerPolicyValue.NoReferrer, "no-referrer" },
                new object[] { ReferrerPolicyValue.NoReferrerWhenDowngrade, "no-referrer-when-downgrade" },
                new object[] { ReferrerPolicyValue.Origin, "origin" },
                new object[] { ReferrerPolicyValue.OriginWhenCrossOrigin, "origin-when-cross-origin" },
                new object[] { ReferrerPolicyValue.SameOrigin, "same-origin" },
                new object[] { ReferrerPolicyValue.StrictOrigin, "strict-origin" },
                new object[] { ReferrerPolicyValue.StrictOriginWhenCrossOrigin, "strict-origin-when-cross-origin" },
                new object[] { ReferrerPolicyValue.UnsafeUrl, "unsafe-url" }
            };

        [Theory]
        [MemberData(nameof(Policies))]
        public async Task AddReferrerPolicyHeader(ReferrerPolicyValue value, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new ReferrerPolicyOptions { Value = value });
            var middleware = new ReferrerPolicyMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.ReferrerPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
