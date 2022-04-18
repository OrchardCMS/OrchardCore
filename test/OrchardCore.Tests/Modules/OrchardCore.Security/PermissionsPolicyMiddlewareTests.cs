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
                new object[] { new[] { PermissionsPolicyValue.Accelerometer }, PermissionsPolicyOriginValue.Any, "accelerometer=*" },
                new object[] { new[] { PermissionsPolicyValue.AmbientLightSensor }, PermissionsPolicyOriginValue.Any, "ambient-light-sensor=*" },
                new object[] { new[] { PermissionsPolicyValue.Autoplay }, PermissionsPolicyOriginValue.Any, "autoplay=*" },
                new object[] { new[] { PermissionsPolicyValue.Camera }, PermissionsPolicyOriginValue.Any, "camera=*" },
                new object[] { new[] { PermissionsPolicyValue.EncryptedMedia }, PermissionsPolicyOriginValue.Any, "encrypted-media=*" },
                new object[] { new[] { PermissionsPolicyValue.FullScreen }, PermissionsPolicyOriginValue.Any, "fullscreen=*" },
                new object[] { new[] { PermissionsPolicyValue.Geolocation }, PermissionsPolicyOriginValue.Any, "geolocation=*" },
                new object[] { new[] { PermissionsPolicyValue.Gyroscope }, PermissionsPolicyOriginValue.Any, "gyroscope=*" },
                new object[] { new[] { PermissionsPolicyValue.Magnetometer }, PermissionsPolicyOriginValue.Any, "magnetometer=*" },
                new object[] { new[] { PermissionsPolicyValue.Microphone }, PermissionsPolicyOriginValue.Any, "microphone=*" },
                new object[] { new[] { PermissionsPolicyValue.Midi }, PermissionsPolicyOriginValue.Any, "midi=*" },
                new object[] { new[] { PermissionsPolicyValue.Notification }, PermissionsPolicyOriginValue.Any, "notification=*" },
                new object[] { new[] { PermissionsPolicyValue.Payment }, PermissionsPolicyOriginValue.Any, "payment=*" },
                new object[] { new[] { PermissionsPolicyValue.PictureInPicture }, PermissionsPolicyOriginValue.Any, "picture-in-picture=*" },
                new object[] { new[] { PermissionsPolicyValue.Push }, PermissionsPolicyOriginValue.Any, "push=*" },
                new object[] { new[] { PermissionsPolicyValue.Speaker }, PermissionsPolicyOriginValue.Any, "speaker=*" },
                new object[] { new[] { PermissionsPolicyValue.SynchronousXhr }, PermissionsPolicyOriginValue.Any, "sync-xhr=*" },
                new object[] { new[] { PermissionsPolicyValue.Usb }, PermissionsPolicyOriginValue.Any, "usb=*" },
                new object[] { new[] { PermissionsPolicyValue.Vibrate }, PermissionsPolicyOriginValue.Any, "vibrate=*" },
                new object[] { new[] { PermissionsPolicyValue.VR }, PermissionsPolicyOriginValue.Any, "vr=*" },
                new object[] { new[] { PermissionsPolicyValue.Camera, PermissionsPolicyValue.Speaker }, PermissionsPolicyOriginValue.Any, "camera=*,speaker=*" },
                new object[] { new[] { PermissionsPolicyValue.Camera, PermissionsPolicyValue.Speaker }, PermissionsPolicyOriginValue.Self, "camera=self,speaker=self" }
            };

        [Theory]
        [MemberData(nameof(Policies))]
        public async Task AddPermissionsPolicyHeader(ICollection<PermissionsPolicyValue> values, PermissionsPolicyOriginValue origin, string expectedValue)
        {
            // Arrange
            var options = Options.Create(new PermissionsPolicyOptions
            {
                Values = values,
                Origin = origin
            });
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
