using System;

namespace OrchardCore.Security.Options
{
    public class PermissionsPolicyOptionsBuilder
    {
        private readonly PermissionsPolicyOptions _options;

        public PermissionsPolicyOptionsBuilder(PermissionsPolicyOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public PermissionsPolicyOptionsBuilder AllowAccelerometer(PermissionsPolicyOriginValue origin)
        {
            _options.Accelerometer.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowAmbientLightSensor(PermissionsPolicyOriginValue origin)
        {
            _options.AmbientLightSensor.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowAutoplay(PermissionsPolicyOriginValue origin)
        {
            _options.Autoplay.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowCamera(PermissionsPolicyOriginValue origin)
        {
            _options.Camera.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowEncryptedMedia(PermissionsPolicyOriginValue origin)
        {
            _options.EncryptedMedia.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowFullScreen(PermissionsPolicyOriginValue origin)
        {
            _options.FullScreen.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowGeolocation(PermissionsPolicyOriginValue origin)
        {
            _options.Geolocation.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowGyroscope(PermissionsPolicyOriginValue origin)
        {
            _options.Gyroscope.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMagnetometer(PermissionsPolicyOriginValue origin)
        {
            _options.Magnetometer.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMicrophone(PermissionsPolicyOriginValue origin)
        {
            _options.Microphone.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMidi(PermissionsPolicyOriginValue origin)
        {
            _options.Midi.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowNotifications(PermissionsPolicyOriginValue origin)
        {
            _options.Notifications.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPayment(PermissionsPolicyOriginValue origin)
        {
            _options.Payment.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPictureInPicture(PermissionsPolicyOriginValue origin)
        {
            _options.Accelerometer.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPush(PermissionsPolicyOriginValue origin)
        {
            _options.Push.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowSpeaker(PermissionsPolicyOriginValue origin)
        {
            _options.Speaker.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowSyncXhr(PermissionsPolicyOriginValue origin)
        {
            _options.SyncXhr.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowUsb(PermissionsPolicyOriginValue origin)
        {
            _options.Usb.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowVibrate(PermissionsPolicyOriginValue origin)
        {
            _options.Vibrate.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowVR(PermissionsPolicyOriginValue origin)
        {
            _options.VR.Origin = origin;

            return this;
        }
    }
}
