using System.Collections.Generic;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        public string[] ContentSecurityPolicy { get; set; }

        public List<string> ContentSecurityPolicyValues { get; set; }

        public bool EnableSandbox { get; set; }

        public bool UpgradeInsecureRequests { get; set; }

        public string FrameOptions { get; set; }

        public string[] PermissionsPolicy { get; set; }

        public List<string> PermissionsPolicyValues { get; set; }

        public string ReferrerPolicy { get; set; }
    }
}
