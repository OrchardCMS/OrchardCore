using OrchardCore.Security.Options;

namespace OrchardCore.Security;

public static class SecurityHeaderDefaults
{
    internal const char PoliciesSeparator = ',';

    internal static readonly string[] ContentSecurityPolicyNames =
    [
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
    ];

    internal static readonly string[] PermissionsPolicyNames =
    [
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
    ];

    public static readonly string[] ContentSecurityPolicy = [];

    public const string ContentTypeOptions = ContentTypeOptionsValue.NoSniff;

    public static readonly string[] PermissionsPolicy = [];

    public const string ReferrerPolicy = ReferrerPolicyValue.NoReferrer;
}
