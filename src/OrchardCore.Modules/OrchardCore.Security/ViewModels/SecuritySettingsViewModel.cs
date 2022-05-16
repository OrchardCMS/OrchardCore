using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        private Dictionary<string, string> _contentSecurityPolicy = new();
        private Dictionary<string, string> _permissionsPolicy = new();

        public Dictionary<string, string> ContentSecurityPolicy
        {
            get => _contentSecurityPolicy;
            set
            {
                // Populate all policy values for the editor (null if not provided).
                _contentSecurityPolicy = SecurityHeaderDefaults.ContentSecurityPolicyNames
                    .ToDictionary(name => name, name =>
                        value?.ContainsKey(name) ?? false
                            ? value[name]
                            : null);
            }
        }

        public bool EnableSandbox { get; set; }

        public bool UpgradeInsecureRequests { get; set; }

        public Dictionary<string, string> PermissionsPolicy
        {
            get => _permissionsPolicy;
            set
            {
                // Populate all policy values for the editor ('None' if not provided).
                _permissionsPolicy = SecurityHeaderDefaults.PermissionsPolicyNames
                    .ToDictionary(name => name, name =>
                        value?.ContainsKey(name) ?? false
                            ? value[name]
                            : PermissionsPolicyOriginValue.None);
            }
        }

        public string ReferrerPolicy { get; set; }

        [BindNever]
        public bool FromConfiguration { get; set; }
    }
}
