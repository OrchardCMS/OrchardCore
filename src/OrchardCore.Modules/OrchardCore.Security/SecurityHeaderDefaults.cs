using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        internal static readonly string PoliciesSeparater = ", ";

        public static readonly string[] ContentSecurityPolicy = new[]
        {
            $"{ContentSecurityPolicyValue.BaseUri} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.ChildSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.ConnectSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.DefaultSource} {ContentSecurityPolicyOriginValue.None}",
            $"{ContentSecurityPolicyValue.FontSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.FormAction} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.FrameAncestors} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.FrameSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.ImageSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.ManifestSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.MediaSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.ObjectSource} {ContentSecurityPolicyOriginValue.Any}",
            $"{ContentSecurityPolicyValue.Sandbox} {SandboxContentSecurityPolicyValue.AllowSameOrigin} {SandboxContentSecurityPolicyValue.AllowScripts}",
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
