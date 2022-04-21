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
                new object[] { new PermissionsPolicyOptions(), "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Accelerometer = new AccelerometerPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=self, ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { AmbientLightSensor = new AmbientLightSensorPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=self, autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Autoplay = new AutoplayPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=self, camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Camera = new CameraPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=self, encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { EncryptedMedia = new EncryptedMediaPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=self, fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { FullScreen = new FullScreenPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=self, geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Geolocation = new GeolocationPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=self, gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Gyroscope = new GyroscopePermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=self, magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Magnetometer = new MagnetometerPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=self, microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Microphone = new MicrophonePermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=self, midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Midi = new MidiPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=*, notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Notifications = new NotificationsPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=*, payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Payment = new PaymentPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=*, picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { PictureInPicture = new PictureInPicturePermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=*, push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Push = new PushPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=*, speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Speaker = new SpeakerPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=*, sync-xhr=(), usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { SyncXhr = new SyncXhrPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=*, usb=(), vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Usb = new UsbPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=*, vibrate=(), vr=()" },
                new object[] { new PermissionsPolicyOptions { Vibrate = new VibratePermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=*, vr=()" },
                new object[] { new PermissionsPolicyOptions { VR = new VrPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=(), encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=(), sync-xhr=(), usb=(), vibrate=(), vr=*" },
                new object[] { new PermissionsPolicyOptions { Camera = new CameraPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Any }, Microphone = new MicrophonePermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self }, Speaker = new SpeakerPermissionsPolicyOptions { Origin = PermissionsPolicyOriginValue.Self } }, "accelerometer=(), ambient-light-sensor=(), autoplay=(), camera=*, encrypted-media=(), fullscreen=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=self, midi=(), notifications=(), payment=(), picture-in-picture=(), push=(), speaker=self, sync-xhr=(), usb=(), vibrate=(), vr=()" },
            };

        [Theory]
        [MemberData(nameof(Policies))]
        public async Task AddPermissionsPolicyHeader(PermissionsPolicyOptions options, string expectedValue)
        {
            // Arrange
            var middleware = new PermissionsPolicyMiddleware(Options.Create(options), request);
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
