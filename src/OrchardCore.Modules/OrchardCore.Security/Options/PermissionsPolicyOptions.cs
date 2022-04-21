using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Security.Options
{
    public class PermissionsPolicyOptions
    {
        private static readonly string _separator = ", ";

        public PermissionsPolicyOriginValue Origin { get; set; } = PermissionsPolicyOriginValue.Self;

        public PermissionsPolicyOptionsBase Accelerometer { get; set; } = new AccelerometerPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase AmbientLightSensor { get; set; } = new AmbientLightSensorPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Autoplay { get; set; } = new AutoplayPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Camera { get; set; } = new CameraPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase EncryptedMedia { get; set; } = new EncryptedMediaPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase FullScreen { get; set; } = new FullScreenPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Geolocation { get; set; } = new GeolocationPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Gyroscope { get; set; } = new GyroscopePermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Magnetometer { get; set; } = new MagnetometerPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Microphone { get; set; } = new MicrophonePermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Midi { get; set; } = new MidiPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Notifications { get; set; } = new NotificationsPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Payment { get; set; } = new PaymentPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase PictureInPicture { get; set; } = new PictureInPicturePermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Push { get; set; } = new PushPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Speaker { get; set; } = new SpeakerPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase SyncXhr { get; set; } = new SyncXhrPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Usb { get; set; } = new UsbPermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase Vibrate { get; set; } = new VibratePermissionsPolicyOptions();

        public PermissionsPolicyOptionsBase VR { get; set; } = new VrPermissionsPolicyOptions();

        public override string ToString()
        {
            var optionValues = new List<PermissionsPolicyOptionsBase>
            {
                Accelerometer,
                AmbientLightSensor,
                Autoplay,
                Camera,
                EncryptedMedia,
                FullScreen,
                Geolocation,
                Gyroscope,
                Magnetometer,
                Microphone,
                Midi,
                Notifications,
                Payment,
                PictureInPicture,
                Push,
                Speaker,
                SyncXhr,
                Usb,
                Vibrate,
                VR
            };

            return String.Join(_separator, optionValues.Select(v => $"{v.Name}={v.Origin}"));
        }
    }
}
