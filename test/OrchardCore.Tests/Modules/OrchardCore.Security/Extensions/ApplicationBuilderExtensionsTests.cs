using System;
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
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(SecurityHeaderDefaults.FrameOptions, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("fullscreen=self", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal($"max-age={SecurityHeaderDefaults.StrictTransportSecurityOptions.MaxAge.TotalSeconds}; includeSubDomains", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
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
                    .AddContentTypeOptions()
                    .AddFrameOptions(FrameOptionsValue.Deny)
                    .AddPermissionsPolicy(new[] { PermissionsPolicyValue.Camera.ToString(), PermissionsPolicyValue.Microphone.ToString() })
                    .AddReferrerPolicy(ReferrerPolicyValue.Origin)
                    .AddStrictTransportSecurity(options => options.MaxAge = TimeSpan.FromMinutes(1));
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(FrameOptionsValue.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("camera=self,microphone=self", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
            Assert.Equal($"max-age=60; includeSubDomains", context.Response.Headers[SecurityHeaderNames.StrictTransportSecurity]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}
