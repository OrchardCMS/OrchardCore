using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Security.Options;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        private Dictionary<string, string> _permissionsPolicy = new();

        public string[] ContentSecurityPolicy { get; set; }

        public List<string> ContentSecurityPolicyValues { get; set; }

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

        public List<string> PermissionsPolicyValues { get; set; }

        public string ReferrerPolicy { get; set; }

        [BindNever]
        public bool FromConfiguration { get; set; }
    }
}
