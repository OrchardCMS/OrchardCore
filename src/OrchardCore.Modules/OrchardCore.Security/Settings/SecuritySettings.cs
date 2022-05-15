using System.Collections.Generic;

namespace OrchardCore.Security.Settings
{
    public class SecuritySettings
    {
        public string[] ContentSecurityPolicy { get; set; } = SecurityHeaderDefaults.ContentSecurityPolicy;

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public IDictionary<string, string> PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public bool FromConfiguration { get; set; }
    }
}
