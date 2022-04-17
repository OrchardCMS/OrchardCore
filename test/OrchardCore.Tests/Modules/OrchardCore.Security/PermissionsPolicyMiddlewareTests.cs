using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OrchardCore.Security.Middlewares;
using Xunit;

namespace OrchardCore.Security.Tests
{
    public class PermissionsPolicyMiddlewareTests
    {
        public static IEnumerable<object[]> Policies =>
            new List<object[]>
            {
                new object[] { new[] { PermissionsPolicyValue.Accelerometer }, "accelerometer=*" },
                new object[] { new[] { PermissionsPolicyValue.AmbientLightSensor }, "ambient-light-sensor=*" },
                new object[] { new[] { PermissionsPolicyValue.Autoplay }, "autoplay=*" },
                new object[] { new[] { PermissionsPolicyValue.Camera }, "camera=*" },
                new object[] { new[] { PermissionsPolicyValue.EncryptedMedia }, "encrypted-media=*" },
                new object[] { new[] { PermissionsPolicyValue.FullScreen }, "fullscreen=*" },
                new object[] { new[] { PermissionsPolicyValue.Geolocation }, "geolocation=*" },
                new object[] { new[] { PermissionsPolicyValue.Gyroscope }, "gyroscope=*" },
                new object[] { new[] { PermissionsPolicyValue.Magnetometer }, "magnetometer=*" },
                new object[] { new[] { PermissionsPolicyValue.Microphone }, "microphone=*" },
                new object[] { new[] { PermissionsPolicyValue.Midi }, "midi=*" },
                new object[] { new[] { PermissionsPolicyValue.Notification }, "notification=*" },
                new object[] { new[] { PermissionsPolicyValue.Payment }, "payment=*" },
                new object[] { new[] { PermissionsPolicyValue.PictureInPicture }, "picture-in-picture=*" },
                new object[] { new[] { PermissionsPolicyValue.Push }, "push=*" },
                new object[] { new[] { PermissionsPolicyValue.Speaker }, "speaker=*" },
                new object[] { new[] { PermissionsPolicyValue.SynchronousXhr }, "sync-xhr=*" },
                new object[] { new[] { PermissionsPolicyValue.Usb }, "usb=*" },
                new object[] { new[] { PermissionsPolicyValue.Vibrate }, "vibrate=*" },
                new object[] { new[] { PermissionsPolicyValue.VR }, "vr=*" },
                new object[] { new[] { PermissionsPolicyValue.Camera, PermissionsPolicyValue.Speaker }, "camera=*,speaker=*" },
            };

        [Theory]
        [MemberData(nameof(Policies))]
        public async Task AddPermissionsPolicyHeader(ICollection<PermissionsPolicyValue> values, string expectedValue)
        {
            // Arrange
            var options = Options.Create(new PermissionsPolicyOptions { Values = values });
            var middleware = new PermissionsPolicyMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.PermissionsPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
