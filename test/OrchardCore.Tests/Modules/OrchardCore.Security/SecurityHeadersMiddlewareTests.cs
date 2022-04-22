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
                ContentTypeOptions = new ContentTypeOptionsOptions()
            });
            var middleware = new SecurityHeadersMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XContentTypeOptions));
            Assert.Equal(ContentTypeOptionsValue.NoSniff, context.Response.Headers[SecurityHeaderNames.XContentTypeOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(FrameOptions))]
        public async Task AddFrameOptionsHeader(string value, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                FrameOptions = new FrameOptionsOptions { Value = value }
            });
            var middleware = new SecurityHeadersMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.XFrameOptions));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.XFrameOptions]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(PermissionsPolicies))]
        public async Task AddPermissionsPolicyHeader(PermissionsPolicyOptions permissionsOptions, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                PermissionsPolicy = permissionsOptions
            });
            var middleware = new SecurityHeadersMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.PermissionsPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.PermissionsPolicy]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }

        [Theory]
        [MemberData(nameof(ReferrerPolicies))]
        public async Task AddReferrerPolicyHeader(string value, string expectedValue)
        {
            // Arrange
            var options = MicrosoftOptions.Create(new SecurityHeadersOptions
            {
                ReferrerPolicy = new ReferrerPolicyOptions { Value = value }
            });
            var middleware = new SecurityHeadersMiddleware(options, request);
            var context = new DefaultHttpContext();

            // Act
            await middleware.Invoke(context);

            // Assert
            Assert.True(context.Response.Headers.ContainsKey(SecurityHeaderNames.ReferrerPolicy));
            Assert.Equal(expectedValue, context.Response.Headers[SecurityHeaderNames.ReferrerPolicy]);

            static Task request(HttpContext context) => Task.CompletedTask;
        }
    }
}
