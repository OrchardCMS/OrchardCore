using Microsoft.Extensions.Primitives;

namespace OrchardCore.Security
{
    public class PermissionsPolicyValue
    {
        private readonly string _option;

        internal PermissionsPolicyValue(string option) => _option = option;

        public static readonly PermissionsPolicyValue Accelerometer = new("accelerometer");

        public static readonly PermissionsPolicyValue AmbientLightSensor = new("ambient-light-sensor");

        public static readonly PermissionsPolicyValue Autoplay = new("autoplay");

        public static readonly PermissionsPolicyValue Camera = new("camera");

        public static readonly PermissionsPolicyValue EncryptedMedia = new("encrypted-media");

        public static readonly PermissionsPolicyValue FullScreen = new("fullscreen");

        public static readonly PermissionsPolicyValue Geolocation = new("geolocation");

        public static readonly PermissionsPolicyValue Gyroscope = new("gyroscope");

        public static readonly PermissionsPolicyValue Magnetometer = new("magnetometer");

        public static readonly PermissionsPolicyValue Microphone = new("microphone");

        public static readonly PermissionsPolicyValue Midi = new("midi");

        public static readonly PermissionsPolicyValue Notifications = new("notifications");

        public static readonly PermissionsPolicyValue Payment = new("payment");

        public static readonly PermissionsPolicyValue PictureInPicture = new("picture-in-picture");

        public static readonly PermissionsPolicyValue Push = new("push");

        public static readonly PermissionsPolicyValue Speaker = new("speaker");

        public static readonly PermissionsPolicyValue SyncXhr = new("sync-xhr");

        public static readonly PermissionsPolicyValue Usb = new("usb");

        public static readonly PermissionsPolicyValue Vibrate = new("vibrate");

        public static readonly PermissionsPolicyValue VR = new("vr");

        public static implicit operator StringValues(PermissionsPolicyValue option) => option.ToString();

        public static implicit operator string(PermissionsPolicyValue option) => option.ToString();

        public override string ToString() => _option;
    }
}
