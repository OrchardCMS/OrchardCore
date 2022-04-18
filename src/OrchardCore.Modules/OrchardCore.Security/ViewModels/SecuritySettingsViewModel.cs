using System.Collections.Generic;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        public string FrameOptions { get; set; }

        public ICollection<string> PermissionsPolicy { get; set; }

        public string PermissionsPolicyOrigin { get; set; }

        public string ReferrerPolicy { get; set; }

        public int StrictTransportSecurityMaxAge { get; set; }

        public bool StrictTransportSecurityIncludeSubDomains { get; set; }

        public bool StrictTransportSecurityPreload { get; set; }
    }
}
