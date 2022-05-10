namespace OrchardCore.Security
{
    public class SecuritySettings
    {
        public string[] ContentSecurityPolicy { get; set; } = SecurityHeaderDefaults.ContentSecurityPolicy;

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string FrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;

        public string[] PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;
    }
}
