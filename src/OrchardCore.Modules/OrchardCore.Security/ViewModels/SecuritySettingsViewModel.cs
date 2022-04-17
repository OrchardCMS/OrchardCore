using System.Collections.Generic;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        public bool AllowSniffing { get; set; }

        public string FrameOptions { get; set; }

        public ICollection<string> PermissionsPolicy { get; set; }

        public string PermissionsPolicyOrigin { get; set; }

        public string ReferrerPolicy { get; set; }
    }
}
