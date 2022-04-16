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
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Accelerometer);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithAmbientLightSensor()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.AmbientLightSensor);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithAutoplay()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Autoplay);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithCamera()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Camera);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithEncryptedMedia()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.EncryptedMedia);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithFederatedLearningOfCohortsCalculation()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.FederatedLearningOfCohortsCalculation);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithFullScreen()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.FullScreen);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithGeolocation()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Geolocation);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithGyroscope()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Gyroscope);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithMagnetometer()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Magnetometer);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithMicrophone()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Microphone);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithMidi()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Midi);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithPayment()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Payment);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithPictureInPicture()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.PictureInPicture);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithSpeaker()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Speaker);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithSynchronousXhr()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.SynchronousXhr);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithUsb()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.Usb);

            return this;
        }

        public PermissionsPolicyHeaderBuilder WithVR()
        {
            _securityHeadersBuilder.AddPermissionsPolicy(PermissionsPolicyOptions.VR);

            return this;
        }

        public SecurityHeadersBuilder Build() => _securityHeadersBuilder;
    }
}
