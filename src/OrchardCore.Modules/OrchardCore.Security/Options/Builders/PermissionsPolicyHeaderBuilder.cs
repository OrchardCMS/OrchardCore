namespace OrchardCore.Security
{
    public class PermissionsPolicyHeaderBuilder
    {
        private readonly SecurityHeadersBuilder _securityHeadersBuilder;

        public PermissionsPolicyHeaderBuilder(SecurityHeadersBuilder securityHeadersBuilder)
        {
            _securityHeadersBuilder = securityHeadersBuilder;
        }

        public PermissionsPolicyHeaderBuilder WithAccelerometer()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Accelerometer);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithAmbientLightSensor()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.AmbientLightSensor);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithAutoplay()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Autoplay);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithCamera()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Camera);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithEncryptedMedia()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.EncryptedMedia);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithFullScreen()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.FullScreen);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithGeolocation()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Geolocation);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithGyroscope()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Gyroscope);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithMagnetometer()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Magnetometer);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithMicrophone()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Microphone);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithMidi()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Midi);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithNotification()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Notification);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithPayment()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Payment);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithPictureInPicture()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.PictureInPicture);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithPush()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Push);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithSpeaker()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Speaker);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithSynchronousXhr()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.SynchronousXhr);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithUsb()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Usb);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithVibrate()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.Vibrate);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithVR()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyValue.VR);

            return this;
        }

        public SecurityHeadersBuilder Build() => _securityHeadersBuilder;
    }
}
