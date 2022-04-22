using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MicrosoftOptions = Microsoft.Extensions.Options.Options;
using OrchardCore.Security.Options;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class SecurityMiddlewareTests
    {
        public static IEnumerable<object[]> FrameOptions =>
            new List<object[]>
            {
                new object[] { FrameOptionsValue.Deny, "DENY" },
                new object[] { FrameOptionsValue.SameOrigin, "SAMEORIGIN" }
            };

        public static IEnumerable<object[]> PermissionsPolicies =>
            new List<object[]>
            {
                new object[] { new[] { $"{PermissionsPolicyValue.Accelerometer}={PermissionsPolicyOriginValue.None}"}, "accelerometer=()" },
                new object[] { new[] { $"{PermissionsPolicyValue.AmbientLightSensor}={PermissionsPolicyOriginValue.Any}"}, "ambient-light-sensor=*" },
                new object[] { new[] { $"{PermissionsPolicyValue.Camera}={PermissionsPolicyOriginValue.Self}"}, "camera=self" },
                new object[] { new[] { $"{PermissionsPolicyValue.EncryptedMedia}={PermissionsPolicyOriginValue.Self} https://www.domain.com" }, "encrypted-media=self https://www.domain.com" },
                new object[] { new[] { $"{PermissionsPolicyValue.FullScreen}={PermissionsPolicyOriginValue.Self} https://www.domain.com https://www.sub.domain.com" }, "fullscreen=self https://www.domain.com https://www.sub.domain.com" },
                new object[] { new[] { $"{PermissionsPolicyValue.Geolocation}={PermissionsPolicyOriginValue.None}", $"{PermissionsPolicyValue.Gyroscope}={PermissionsPolicyOriginValue.Any}" }, "geolocation=(), gyroscope=*" },
                new object[] { new[] { $"{PermissionsPolicyValue.Magnetometer}={PermissionsPolicyOriginValue.None}", $"{PermissionsPolicyValue.Microphone}={PermissionsPolicyOriginValue.Any}", $"{PermissionsPolicyValue.Midi}={PermissionsPolicyOriginValue.Self}" }, "magnetometer=(), microphone=*, midi=self" },
                new object[] { new[] { $"{PermissionsPolicyValue.Notifications}={PermissionsPolicyOriginValue.Self}", $"{PermissionsPolicyValue.Payment}={PermissionsPolicyOriginValue.Self} https://www.domain.com", $"{PermissionsPolicyValue.PictureInPicture}={PermissionsPolicyOriginValue.Self}", $"{PermissionsPolicyValue.Push}={PermissionsPolicyOriginValue.Self} https://www.domain.com https://www.sub.domain.com" }, "notifications=self, payment=self https://www.domain.com, picture-in-picture=self, push=self https://www.domain.com https://www.sub.domain.com" }
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

        [Fact]
        public async Task AddContentTypeOptionsHeader()
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                ContentTypeOptions = ContentTypeOptionsValue.NoSniff
            });
            var middleware = new SecurityHeadersMiddleware(options, Request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XContentTypeOptions));
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);
        }

        [Theory]
        [MemberData(nameof(FrameOptions))]
        public async Task AddFrameOptionsHeader(string option, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                FrameOptions = option
            });
            var middleware = new SecurityHeadersMiddleware(options, Request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);
        }

        [Theory]
        [MemberData(nameof(PermissionsPolicies))]
        public async Task AddPermissionsPolicyHeader(string[] permissionsOptions, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                PermissionsPolicy = permissionsOptions
            });
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
        public async Task AddReferrerPolicyHeader(string policy, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                ReferrerPolicy = policy
            });
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
