using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class PermissionsPolicyBuilderExtensionsTests
    {
        [Fact]
        public void UsePermissionsPolicyWithOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new PermissionsPolicyOptions {
                Values = new[] { PermissionsPolicyValue.Camera, PermissionsPolicyValue.Speaker },
                Origin = PermissionsPolicyOriginValue.Self
            };
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UsePermissionsPolicy(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("camera=self,speaker=self", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}
