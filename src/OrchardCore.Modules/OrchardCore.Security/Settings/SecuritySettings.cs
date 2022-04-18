using System.Collections.Generic;

namespace OrchardCore.Security
{
    public class SecuritySettings
    {
        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string FrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;

        public ICollection<string> PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string PermissionsPolicyOrigin { get; set; }

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public StrictTransportSecurityOptions StrictTransportSecurity { get; set; } = SecurityHeaderDefaults.StrictTransportSecurityOptions;
    }
}
