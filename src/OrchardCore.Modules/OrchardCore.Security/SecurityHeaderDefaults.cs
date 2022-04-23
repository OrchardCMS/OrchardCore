using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        public static readonly string[] ContentSecurityPolicy = new[]
        {
            $"{ContentSecurityPolicyValue.BaseUri} {ContentSecurityPolicyOriginValue.None}",
            $"{ContentSecurityPolicyValue.ChildSource} {ContentSecurityPolicyOriginValue.None}",
            $"{ContentSecurityPolicyValue.ConnectSource} {ContentSecurityPolicyOriginValue.None}",
            $"{ContentSecurityPolicyValue.DefaultSource} {ContentSecurityPolicyOriginValue.None}",
            $"{ContentSecurityPolicyValue.FontSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.FormAction} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.FrameAncestors} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.FrameSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.ImageSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.ManifestSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.MediaSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.ObjectSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.Sandbox}",
            $"{ContentSecurityPolicyValue.ScriptSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.StyleSource} {ContentSecurityPolicyOriginValue.Self}",
            $"{ContentSecurityPolicyValue.UpgradeInsecureRequests}",
        };

        public static readonly string ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

        public static readonly string FrameOptions = FrameOptionsValue.SameOrigin;

        public static readonly string[] PermissionsPolicy = new[]
        {
            $"{PermissionsPolicyValue.Accelerometer}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.AmbientLightSensor}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Autoplay}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Camera}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.EncryptedMedia}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.FullScreen}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Geolocation}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Gyroscope}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Magnetometer}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Microphone}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Midi}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Notifications}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Payment}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.PictureInPicture}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Push}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Speaker}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.SyncXhr}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Usb}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.Vibrate}={PermissionsPolicyOriginValue.None}",
            $"{PermissionsPolicyValue.VR}={PermissionsPolicyOriginValue.None}"
        };

        public static readonly string ReferrerPolicy = ReferrerPolicyValue.NoReferrer;
    }
}
