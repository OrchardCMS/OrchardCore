using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.Settings
{
    public class SecuritySettings
    {
        private Dictionary<string, string> _securityPolicy = new();
        private Dictionary<string, string> _permissionsPolicy = new();

        public Dictionary<string, string> ContentSecurityPolicy
        {
            get => _securityPolicy;
            set
            {
                if (value == null)
                {
                    return;
                }

                // Exlude null values and clone the dictionary to not be shared by site settings and options instances.
                _securityPolicy = value
                    .Where(kvp => kvp.Value != null ||
                        kvp.Key == ContentSecurityPolicyValue.Sandbox ||
                        kvp.Key == ContentSecurityPolicyValue.UpgradeInsecureRequests)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                if (_securityPolicy.TryGetValue(ContentSecurityPolicyValue.UpgradeInsecureRequests, out _))
                {
                    _securityPolicy[ContentSecurityPolicyValue.UpgradeInsecureRequests] = null;
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

        [JsonIgnore]
        public bool FromConfiguration { get; set; }
    }
}
