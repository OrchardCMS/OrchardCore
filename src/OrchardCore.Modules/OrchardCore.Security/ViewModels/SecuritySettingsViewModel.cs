using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        public string[] ContentSecurityPolicy { get; set; }

        public List<string> AllContentSecurityPolicy { get; set; }

        public bool EnableSandbox { get; set; }

        public bool UpgradeInsecureRequests { get; set; }

        public IDictionary<string, string> PermissionsPolicy { get; set; }

        public IDictionary<string, string> AllPermissionsPolicy { get; set; }

        public string ReferrerPolicy { get; set; }

        [BindNever]
        public bool FromAdminSettings { get; set; } = true;
    }
}
