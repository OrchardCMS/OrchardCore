using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class PermissionsPolicyMiddlewareTests
    {
        [Fact]
        public async Task AddPermissionsPolicyHeader()
        {
            // Arrange
            var policies = new [] { PermissionsPolicyOptions.Camera, PermissionsPolicyOptions.Microphone };
            var middleware = new PermissionsPolicyMiddleware(policies, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            var permissionsPolicy = context.Response.Headers[SecurityHeaderNames.PermissionsPolicy].ToString();

            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.PermissionsPolicy));
            Assert.True(permissionsPolicy.IndexOf(PermissionsPolicyOptions.Camera) > -1);
            Assert.True(permissionsPolicy.IndexOf(PermissionsPolicyOptions.Microphone) > -1);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
