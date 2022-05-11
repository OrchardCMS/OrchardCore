using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security.Options;
using Xunit;

namespace OrchardCore.Security.Extensions.Tests
{
    public class SecurityHeadersApplicationBuilderExtensionsTests
    {
        [Fact]
        public void AddHeaders_Defaults()
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
            Assert.Equal(SecurityHeaderDefaults.ContentSecurityPolicy, context.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy]);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(SecurityHeaderDefaults.FrameOptions, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal(SecurityHeaderDefaults.PermissionsPolicy, context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        [Fact]
        public void AddHeaders_Options()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new SecurityHeadersOptions
            {
                ContentSecurityPolicy = new []
                {
                    $"{ContentSecurityPolicyValue.ChildSource} {ContentSecurityPolicyOriginValue.None}",
                    $"{ContentSecurityPolicyValue.ConnectSource} {ContentSecurityPolicyOriginValue.Self} https://www.domain1.com https://www.domain2.com",
                    $"{ContentSecurityPolicyValue.DefaultSource} {ContentSecurityPolicyOriginValue.Any}",
                },
                ContentTypeOptions = ContentTypeOptionsValue.NoSniff,
                FrameOptions = FrameOptionsValue.Deny,
                PermissionsPolicy = new []
                {
                    $"{PermissionsPolicyValue.Camera}={PermissionsPolicyOriginValue.Self}",
                    $"{PermissionsPolicyValue.Microphone}={PermissionsPolicyOriginValue.Any}",
                    $"{PermissionsPolicyValue.Speaker}={PermissionsPolicyOriginValue.Self} https://www.domain1.com https://www.domain2.com"
                },
                ReferrerPolicy = ReferrerPolicyValue.Origin
            };
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(options);

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("child-src 'none',connect-src 'self' https://www.domain1.com https://www.domain2.com,default-src *", context.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy]);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(FrameOptionsValue.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("camera=self,microphone=*,speaker=self https://www.domain1.com https://www.domain2.com", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        [Fact]
        public void AddHeaders_BuilderConfiguration()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var applicationBuilder = CreateApplicationBuilder();

            // Act
            applicationBuilder.UseSecurityHeaders(config =>
            {
                config
                    .AddContentSecurityPolicy("child-src 'none'", "connect-src 'self' https://www.domain1.com https://www.domain2.com", "default-src *")
                    .AddContentTypeOptions()
                    .AddFrameOptions(FrameOptionsValue.Deny)
                    .AddPermissionsPolicy("camera=self", "microphone=*", "speaker=self https://www.domain1.com https://www.domain2.com")
                    .AddReferrerPolicy(ReferrerPolicyValue.Origin);
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("child-src 'none',connect-src 'self' https://www.domain1.com https://www.domain2.com,default-src *", context.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy]);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
            Assert.Equal(FrameOptionsValue.Deny, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
            Assert.Equal("camera=self,microphone=*,speaker=self https://www.domain1.com https://www.domain2.com", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        private static IApplicationBuilder CreateApplicationBuilder()
        {
            var services = new ServiceCollection();
            var serviceProvider = services.BuildServiceProvider();

            return new ApplicationBuilder(serviceProvider);
        }
    }
}
