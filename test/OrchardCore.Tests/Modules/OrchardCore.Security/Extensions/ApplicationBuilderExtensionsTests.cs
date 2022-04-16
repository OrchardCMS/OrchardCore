using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class ApplicationBuilderExtensionsTests
    {
        [Fact]
        public void UseSecurityHeadersWithDefaultHeaders()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders();

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal(SecurityHeaderDefaults.FrameOptions, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
        }

        [Fact]
        public void UseSecurityHeadersWithConfigureBuilder()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(config =>
            {
                config
                    .AddReferrerPolicy(ReferrerPolicyOptions.Origin)
                    .AddFrameOptions(FrameOptions.Deny)
                    .AddPermissionsPolicy(new[] { PermissionsPolicyOptions.Camera, PermissionsPolicyOptions.Microphone });
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            var permissionsPolicy = context.Response.Headers[SecurityHeaderNames.PermissionsPolicy].ToString();

            Assert.Equal(ReferrerPolicyOptions.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal(FrameOptions.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.True(permissionsPolicy.IndexOf(PermissionsPolicyOptions.Camera) > -1);
            Assert.True(permissionsPolicy.IndexOf(PermissionsPolicyOptions.Microphone) > -1);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}
