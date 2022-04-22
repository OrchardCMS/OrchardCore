using System.Collections.Generic;
using OrchardCore.Security.Options;

namespace OrchardCore.Security
{
    public class SecuritySettings
    {
        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string FrameOptions { get; set; } = SecurityHeaderDefaults.FrameOptions;

        public ICollection<string> PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string PermissionsPolicyOrigin { get; set; } = PermissionsPolicyOriginValue.Self;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;
    }
}
