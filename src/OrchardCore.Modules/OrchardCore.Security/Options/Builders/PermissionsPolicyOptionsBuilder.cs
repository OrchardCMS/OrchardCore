using System;

namespace OrchardCore.Security.Options
{
    public class PermissionsPolicyOptionsBuilder
    {
        private readonly PermissionsPolicyOptions _options;

        public PermissionsPolicyOptionsBuilder(PermissionsPolicyOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public PermissionsPolicyOptionsBuilder AllowAccelerometer(string origin)
        {
            _options.Accelerometer.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowAmbientLightSensor(string origin)
        {
            _options.AmbientLightSensor.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowAutoplay(string origin)
        {
            _options.Autoplay.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowCamera(string origin)
        {
            _options.Camera.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowEncryptedMedia(string origin)
        {
            _options.EncryptedMedia.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowFullScreen(string origin)
        {
            _options.FullScreen.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowGeolocation(string origin)
        {
            _options.Geolocation.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowGyroscope(string origin)
        {
            _options.Gyroscope.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMagnetometer(string origin)
        {
            _options.Magnetometer.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMicrophone(string origin)
        {
            _options.Microphone.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMidi(string origin)
        {
            _options.Midi.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowNotifications(string origin)
        {
            _options.Notifications.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPayment(string origin)
        {
            _options.Payment.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPictureInPicture(string origin)
        {
            _options.Accelerometer.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPush(string origin)
        {
            _options.Push.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowSpeaker(string origin)
        {
            _options.Speaker.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowSyncXhr(string origin)
        {
            _options.SyncXhr.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowUsb(string origin)
        {
            _options.Usb.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowVibrate(string origin)
        {
            _options.Vibrate.Origin = origin;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowVR(string origin)
        {
            _options.VR.Origin = origin;

            return this;
        }
    }
}
