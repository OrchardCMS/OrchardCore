using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class ApplicationBuilderExtensionsTests
    {
        [Fact(Skip = "Unskip the test if UseSecurityHeaders() is ready.")]
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

        [Fact(Skip = "Unskip the test if UseSecurityHeaders() is ready.")]
        public void UseSecurityHeadersWithConfigureBuilder()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(config =>
            {
                config
                    .AddReferrerPolicy(ReferrerPolicyValue.Origin)
                    .AddFrameOptions(FrameOptionsValue.Deny)
                    .AddPermissionsPolicy(new[] { PermissionsPolicyValue.Camera.ToString(), PermissionsPolicyValue.Microphone.ToString() });
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            var permissionsPolicy = context.Response.Headers[SecurityHeaderNames.PermissionsPolicy].ToString();

            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal(FrameOptionsValue.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.True(permissionsPolicy.IndexOf(PermissionsPolicyValue.Camera) > -1);
            Assert.True(permissionsPolicy.IndexOf(PermissionsPolicyValue.Microphone) > -1);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}
