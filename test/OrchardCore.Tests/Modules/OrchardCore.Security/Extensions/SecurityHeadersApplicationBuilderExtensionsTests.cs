using OrchardCore.Security.Options;

namespace OrchardCore.Security.Extensions.Tests
{
    public class SecurityHeadersApplicationBuilderExtensionsTests
    {
        [Fact]
        public void SecurityHeadersShouldBeAddedWithDefaultValues()
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
            Assert.Equal(SecurityHeaderDefaults.PermissionsPolicy, context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(SecurityHeaderDefaults.ReferrerPolicy, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        [Fact]
        public void SecurityHeadersShouldAddedAccordingSuppliedOptions()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var options = new SecurityHeadersOptions
            {
                ContentSecurityPolicy = new[]
                {
                    $"{ContentSecurityPolicyValue.ChildSource} {ContentSecurityPolicyOriginValue.None}",
                    $"{ContentSecurityPolicyValue.ConnectSource} {ContentSecurityPolicyOriginValue.Self} https://www.domain1.com https://www.domain2.com",
                    $"{ContentSecurityPolicyValue.DefaultSource} {ContentSecurityPolicyOriginValue.Any}",
                },
                ContentTypeOptions = ContentTypeOptionsValue.NoSniff,
                PermissionsPolicy = new[]
                {
                    $"{PermissionsPolicyValue.Camera}={PermissionsPolicyOriginValue.Self}",
                    $"{PermissionsPolicyValue.Microphone}={PermissionsPolicyOriginValue.Any}",
                    $"{PermissionsPolicyValue.SpeakerSelection}={PermissionsPolicyOriginValue.Self} https://www.domain1.com https://www.domain2.com"
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
            Assert.Equal("camera=self,microphone=*,speaker-selection=self https://www.domain1.com https://www.domain2.com", context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
            Assert.Equal(ReferrerPolicyValue.Origin, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        [Fact]
        public void SecurityHeadersShouldBeAddedAccordingBuildConfiguration()
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
                    .AddPermissionsPolicy(new Dictionary<string, string>
                    {
                        { "camera", "self"},
                        { "microphone", "*" },
                        { "speaker", "self https://www.domain1.com https://www.domain2.com"}
                    })
                    .AddReferrerPolicy(ReferrerPolicyValue.Origin);
            });

            applicationBuilder
                .Build()
                .Invoke(context);

            // Assert
            Assert.Equal("child-src 'none',connect-src 'self' https://www.domain1.com https://www.domain2.com,default-src *", context.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy]);
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
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
