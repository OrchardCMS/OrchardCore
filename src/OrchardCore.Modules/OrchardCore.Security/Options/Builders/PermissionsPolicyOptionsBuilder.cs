using System;

namespace OrchardCore.Security.Options
{
    public class PermissionsPolicyOptionsBuilder
    {
        private readonly PermissionsPolicyOptions _options;

        public PermissionsPolicyOptionsBuilder(PermissionsPolicyOptions options)
            => _options = options ?? throw new ArgumentNullException(nameof(options));

        public PermissionsPolicyOptionsBuilder AllowAccelerometer(string origin, params string[] allowedOrigins)
        {
            _options.Accelerometer.Origin = origin;
            _options.Accelerometer.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowAmbientLightSensor(string origin, params string[] allowedOrigins)
        {
            _options.AmbientLightSensor.Origin = origin;
            _options.AmbientLightSensor.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowAutoplay(string origin, params string[] allowedOrigins)
        {
            _options.Autoplay.Origin = origin;
            _options.Autoplay.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowCamera(string origin, params string[] allowedOrigins)
        {
            _options.Camera.Origin = origin;
            _options.Camera.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowEncryptedMedia(string origin, params string[] allowedOrigins)
        {
            _options.EncryptedMedia.Origin = origin;
            _options.EncryptedMedia.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowFullScreen(string origin, params string[] allowedOrigins)
        {
            _options.FullScreen.Origin = origin;
            _options.FullScreen.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowGeolocation(string origin, params string[] allowedOrigins)
        {
            _options.Geolocation.Origin = origin;
            _options.Geolocation.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowGyroscope(string origin, params string[] allowedOrigins)
        {
            _options.Gyroscope.Origin = origin;
            _options.Gyroscope.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMagnetometer(string origin, params string[] allowedOrigins)
        {
            _options.Magnetometer.Origin = origin;
            _options.Magnetometer.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMicrophone(string origin, params string[] allowedOrigins)
        {
            _options.Microphone.Origin = origin;
            _options.Microphone.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowMidi(string origin, params string[] allowedOrigins)
        {
            _options.Midi.Origin = origin;
            _options.Midi.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowNotifications(string origin, params string[] allowedOrigins)
        {
            _options.Notifications.Origin = origin;
            _options.Notifications.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPayment(string origin, params string[] allowedOrigins)
        {
            _options.Payment.Origin = origin;
            _options.Payment.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPictureInPicture(string origin, params string[] allowedOrigins)
        {
            _options.Accelerometer.Origin = origin;
            _options.PictureInPicture.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowPush(string origin, params string[] allowedOrigins)
        {
            _options.Push.Origin = origin;
            _options.Push.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowSpeaker(string origin, params string[] allowedOrigins)
        {
            _options.Speaker.Origin = origin;
            _options.Speaker.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowSyncXhr(string origin, params string[] allowedOrigins)
        {
            _options.SyncXhr.Origin = origin;
            _options.SyncXhr.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowUsb(string origin, params string[] allowedOrigins)
        {
            _options.Usb.Origin = origin;
            _options.Usb.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowVibrate(string origin, params string[] allowedOrigins)
        {
            _options.Vibrate.Origin = origin;
            _options.Vibrate.AllowedOrigins = allowedOrigins;

            return this;
        }

        public PermissionsPolicyOptionsBuilder AllowVR(string origin, params string[] allowedOrigins)
        {
            _options.VR.Origin = origin;
            _options.VR.AllowedOrigins = allowedOrigins;

            return this;
        }
    }
}
