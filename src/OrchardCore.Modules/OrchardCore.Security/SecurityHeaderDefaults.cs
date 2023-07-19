using System;
using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public static class SecurityHeaderDefaults
    {
        internal static readonly char PoliciesSeparator = ',';
        internal static readonly string[] ContentSecurityPolicyNames = new[]
        {
            ContentSecurityPolicyValue.BaseUri,
            ContentSecurityPolicyValue.ChildSource,
            ContentSecurityPolicyValue.ConnectSource,
            ContentSecurityPolicyValue.DefaultSource,
            ContentSecurityPolicyValue.FontSource,
            ContentSecurityPolicyValue.FormAction,
            ContentSecurityPolicyValue.FrameAncestors,
            ContentSecurityPolicyValue.FrameSource,
            ContentSecurityPolicyValue.ImageSource,
            ContentSecurityPolicyValue.ManifestSource,
            ContentSecurityPolicyValue.MediaSource,
            ContentSecurityPolicyValue.ObjectSource,
            ContentSecurityPolicyValue.ScriptSource,
            ContentSecurityPolicyValue.StyleSource,
            ContentSecurityPolicyValue.Sandbox
        };
        internal static readonly string[] PermissionsPolicyNames = new[]
        {
            PermissionsPolicyValue.Accelerometer,
            PermissionsPolicyValue.AmbientLightSensor,
            PermissionsPolicyValue.Autoplay,
            PermissionsPolicyValue.Battery,
            PermissionsPolicyValue.Camera,
            PermissionsPolicyValue.DisplayCapture,
            PermissionsPolicyValue.DocumentDomain,
            PermissionsPolicyValue.EncryptedMedia,
            PermissionsPolicyValue.FullScreen,
            PermissionsPolicyValue.GamePad,
            PermissionsPolicyValue.Geolocation,
            PermissionsPolicyValue.Gyroscope,
            PermissionsPolicyValue.LayoutAnimations,
            PermissionsPolicyValue.LegacyImageFormat,
            PermissionsPolicyValue.Magnetometer,
            PermissionsPolicyValue.Microphone,
            PermissionsPolicyValue.Midi,
            PermissionsPolicyValue.OversizedImages,
            PermissionsPolicyValue.Payment,
            PermissionsPolicyValue.PictureInPicture,
            PermissionsPolicyValue.PublicKeyRetrieval,
            PermissionsPolicyValue.SpeakerSelection,
            PermissionsPolicyValue.ScreenWakeLock,
            PermissionsPolicyValue.SyncXhr,
            PermissionsPolicyValue.UnoptimizedImages,
            PermissionsPolicyValue.UnsizedMedia,
            PermissionsPolicyValue.Usb,
            PermissionsPolicyValue.WebShare,
            PermissionsPolicyValue.WebXR
        };

        public static readonly string[] ContentSecurityPolicy = Array.Empty<string>();

        public static readonly string ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

        public static readonly string[] PermissionsPolicy = Array.Empty<string>();

        public static readonly string ReferrerPolicy = ReferrerPolicyValue.NoReferrer;
    }
}
