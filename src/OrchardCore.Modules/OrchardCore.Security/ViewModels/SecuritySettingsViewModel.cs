using System.Collections.Generic;

namespace OrchardCore.Security.ViewModels
{
    public class SecuritySettingsViewModel
    {
        public string ReferrerPolicy { get; set; }

        public string XFrameOptions { get; set; }

        public IList<string> PermissionsPolicy { get; set; }
    }
}
