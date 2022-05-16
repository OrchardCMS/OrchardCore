using System.Collections.Generic;
using System.Linq;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Settings
{
    public class SecuritySettings
    {
        private Dictionary<string, string> _contentSecurityPolicy = new();
        private Dictionary<string, string> _permissionsPolicy = new();

        public Dictionary<string, string> ContentSecurityPolicy
        {
            get => _contentSecurityPolicy;
            set
            {
                if (value == null)
                {
                    return;
                }

                // Exclude null values and clone the dictionary to not be shared by site settings and options instances.
                _contentSecurityPolicy = value
                    .Where(kvp => kvp.Value != null ||
                        kvp.Key == ContentSecurityPolicyValue.Sandbox ||
                        kvp.Key == ContentSecurityPolicyValue.UpgradeInsecureRequests)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (_contentSecurityPolicy.TryGetValue(ContentSecurityPolicyValue.UpgradeInsecureRequests, out _))
                {
                    _contentSecurityPolicy[ContentSecurityPolicyValue.UpgradeInsecureRequests] = null;
                }
            }
        }

        public string ContentTypeOptions { get; set; } = SecurityHeaderDefaults.ContentTypeOptions;

        public Dictionary<string, string> PermissionsPolicy
        {
            get => _permissionsPolicy;
            set
            {
                if (value == null)
                {
                    return;
                }

                // Exlude 'None' values and clone the dictionary to not be shared by site settings and options instances.
                _permissionsPolicy = value
                    .Where(kvp => kvp.Value != PermissionsPolicyOriginValue.None)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
        }

        public string ReferrerPolicy { get; set; } = SecurityHeaderDefaults.ReferrerPolicy;

        public bool FromConfiguration { get; set; }
    }
}
