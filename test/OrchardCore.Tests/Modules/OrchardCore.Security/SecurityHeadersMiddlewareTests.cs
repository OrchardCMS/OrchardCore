using OrchardCore.Security.Options;
using OrchardCore.Security.Services;

namespace OrchardCore.Security.Tests
{
    public class SecurityMiddlewareTests
    {
        public static IEnumerable<object[]> ContentSecurityPolicies =>
            new List<object[]>
            {
                new object[] { new[] { $"{ContentSecurityPolicyValue.BaseUri} {ContentSecurityPolicyOriginValue.None}"}, "base-uri 'none'" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.ChildSource} {ContentSecurityPolicyOriginValue.Self}"}, "child-src 'self'" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.ConnectSource} {ContentSecurityPolicyOriginValue.Self} https://www.domain.com/" }, "connect-src 'self' https://www.domain.com/" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.DefaultSource} {ContentSecurityPolicyOriginValue.Self} https://www.domain1.com/ https://www.domain2.com/" }, "default-src 'self' https://www.domain1.com/ https://www.domain2.com/" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.FontSource} {ContentSecurityPolicyOriginValue.Any}"}, "font-src *" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.ScriptSource} {ContentSecurityPolicyOriginValue.Self} https://www.domain.com/", $"{ContentSecurityPolicyValue.StyleSource} {ContentSecurityPolicyOriginValue.Self} https://www.domain.com/" }, "script-src 'self' https://www.domain.com/,style-src 'self' https://www.domain.com/" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.Sandbox}"}, "sandbox" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.Sandbox} allow-scripts" }, "sandbox allow-scripts" },
                new object[] { new[] { $"{ContentSecurityPolicyValue.UpgradeInsecureRequests}"}, "upgrade-insecure-requests" },
            };

        public static IEnumerable<object[]> PermissionsPolicies =>
            new List<object[]>
            {
                new object[] { new[] { $"{PermissionsPolicyValue.Accelerometer}={PermissionsPolicyOriginValue.None}"}, "accelerometer=()" },
                new object[] { new[] { $"{PermissionsPolicyValue.AmbientLightSensor}={PermissionsPolicyOriginValue.Any}"}, "ambient-light-sensor=*" },
                new object[] { new[] { $"{PermissionsPolicyValue.Camera}={PermissionsPolicyOriginValue.Self}"}, "camera=self" },
                new object[] { new[] { $"{PermissionsPolicyValue.EncryptedMedia}={PermissionsPolicyOriginValue.Self} https://www.domain.com" }, "encrypted-media=self https://www.domain.com" },
                new object[] { new[] { $"{PermissionsPolicyValue.FullScreen}={PermissionsPolicyOriginValue.Self} https://www.domain.com https://www.sub.domain.com" }, "fullscreen=self https://www.domain.com https://www.sub.domain.com" },
                new object[] { new[] { $"{PermissionsPolicyValue.Geolocation}={PermissionsPolicyOriginValue.None}", $"{PermissionsPolicyValue.Gyroscope}={PermissionsPolicyOriginValue.Any}" }, "geolocation=(),gyroscope=*" },
                new object[] { new[] { $"{PermissionsPolicyValue.Magnetometer}={PermissionsPolicyOriginValue.None}", $"{PermissionsPolicyValue.Microphone}={PermissionsPolicyOriginValue.Any}", $"{PermissionsPolicyValue.Midi}={PermissionsPolicyOriginValue.Self}" }, "magnetometer=(),microphone=*,midi=self" },
                new object[] { new[] { $"{PermissionsPolicyValue.Usb}={PermissionsPolicyOriginValue.Self}", $"{PermissionsPolicyValue.Payment}={PermissionsPolicyOriginValue.Self} https://www.domain.com", $"{PermissionsPolicyValue.PictureInPicture}={PermissionsPolicyOriginValue.Self}", $"{PermissionsPolicyValue.Push}={PermissionsPolicyOriginValue.Self} https://www.domain.com https://www.sub.domain.com" }, "usb=self,payment=self https://www.domain.com,picture-in-picture=self,push=self https://www.domain.com https://www.sub.domain.com" }
            };

        public static IEnumerable<object[]> ReferrerPolicies =>
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
        [MemberData(nameof(ContentSecurityPolicies))]
        public async Task ContentSecurityPolicyHeaderShouldBeAdded(string[] contentSecurityPolicies, string expectedValue)
        {
            // Arrange
            var options = new SecurityHeadersOptions
            {
                ContentSecurityPolicy = contentSecurityPolicies
            };
            var middleware = new SecurityHeadersMiddleware(options, Request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.ContentSecurityPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.ContentSecurityPolicy]);
        }

        [Fact]
        public async Task ContentTypeOptionsHeaderShouldBeAdded()
        {
            // Arrange
            var options = new SecurityHeadersOptions
            {
                ContentTypeOptions = ContentTypeOptionsValue.NoSniff
            };
            var middleware = new SecurityHeadersMiddleware(options, Request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XContentTypeOptions));
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
        }

        [Theory]
        [MemberData(nameof(PermissionsPolicies))]
        public async Task PermissionsPolicyHeaderShouldBeAdded(string[] permissionsPolicies, string expectedValue)
        {
            // Arrange
            var options = new SecurityHeadersOptions
            {
                PermissionsPolicy = permissionsPolicies
            };
            var middleware = new SecurityHeadersMiddleware(options, Request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.PermissionsPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);
        }

        [Theory]
        [MemberData(nameof(ReferrerPolicies))]
        public async Task ReferrerPolicyHeaderShouldBeAdded(string policy, string expectedValue)
        {
            // Arrange
            var options = new SecurityHeadersOptions
            {
                ReferrerPolicy = policy
            };
            var middleware = new SecurityHeadersMiddleware(options, Request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.ReferrerPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);
        }

        private static Task Request(HttpContext context) => Task.CompletedTask;
    }
}
