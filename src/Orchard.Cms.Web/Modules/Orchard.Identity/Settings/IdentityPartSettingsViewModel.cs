using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.Identity.Settings
{
    public class IdentityPartSettingsViewModel
    {
        public bool ShowIdentityEditor { get; set; }
        public bool AllowCustomIdentity { get; set; }
        public bool ShowNameEditor { get; set; }
        public bool AllowChangeIdentity { get; set; }

        [BindNever]
        public IdentityPartSettings IdentityPartSettings { get; set; }
    }
}
