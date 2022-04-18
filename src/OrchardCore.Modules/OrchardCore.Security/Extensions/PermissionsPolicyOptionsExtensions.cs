using System;

namespace OrchardCore.Security
{
    public static class PermissionsPolicyOptionsExtensions
    {
        public static PermissionsPolicyOptions AllowAccelerometer(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Accelerometer);

            return options;
        }

        public static PermissionsPolicyOptions AllowAmbientLightSensor(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.AmbientLightSensor);

            return options;
        }

        public static PermissionsPolicyOptions AllowAutoplay(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Autoplay);

            return options;
        }

        public static PermissionsPolicyOptions AllowCamera(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Camera);

            return options;
        }

        public static PermissionsPolicyOptions AllowEncryptedMedia(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.EncryptedMedia);

            return options;
        }

        public static PermissionsPolicyOptions AllowFullScreen(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.FullScreen);

            return options;
        }

        public static PermissionsPolicyOptions AllowGeolocation(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Geolocation);

            return options;
        }

        public static PermissionsPolicyOptions AllowGyroscope(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Gyroscope);

            return options;
        }

        public static PermissionsPolicyOptions AllowMagnetometer(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Magnetometer);

            return options;
        }

        public static PermissionsPolicyOptions AllowMicrophone(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Microphone);

            return options;
        }

        public static PermissionsPolicyOptions AllowMidi(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Midi);

            return options;
        }

        public static PermissionsPolicyOptions AllowNotification(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Notification);

            return options;
        }

        public static PermissionsPolicyOptions AllowPayment(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Payment);

            return options;
        }

        public static PermissionsPolicyOptions AllowPictureInPicture(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.PictureInPicture);

            return options;
        }

        public static PermissionsPolicyOptions AllowPush(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Push);

            return options;
        }

        public static PermissionsPolicyOptions AllowSpeaker(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Speaker);

            return options;
        }

        public static PermissionsPolicyOptions AllowSynchronousXhr(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.SynchronousXhr);

            return options;
        }

        public static PermissionsPolicyOptions AllowUsb(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Usb);

            return options;
        }

        public static PermissionsPolicyOptions AllowVibrate(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.Vibrate);

            return options;
        }

        public static PermissionsPolicyOptions AllowVR(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Values.Add(PermissionsPolicyValue.VR);

            return options;
        }

        public static PermissionsPolicyOptions WithAnyOrigin(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Origin = PermissionsPolicyOriginValue.Any;

            return options;
        }

        public static PermissionsPolicyOptions WithSelfOrigin(this PermissionsPolicyOptions options)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            options.Origin = PermissionsPolicyOriginValue.Self;

            return options;
        }
    }
}
