using System;
using System.Linq;

namespace OrchardCore.Security.Settings
{
    public class SecuritySettings : IEquatable<SecuritySettings>
    {
        public string[] ContentSecurityPolicy { get; set; } = SecurityHeaderDefaults.ContentSecurityPolicy;

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public string[] PermissionsPolicy { get; set; } = SecurityHeaderDefaults.PermissionsPolicy;

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public bool Equals(SecuritySettings other)
            => ContentSecurityPolicy.SequenceEqual(other.ContentSecurityPolicy) &&
                ContentTypeOptions == other.ContentTypeOptions &&
                PermissionsPolicy.SequenceEqual(other.PermissionsPolicy) &&
                ReferrerPolicy == other.ReferrerPolicy;
    }
}
