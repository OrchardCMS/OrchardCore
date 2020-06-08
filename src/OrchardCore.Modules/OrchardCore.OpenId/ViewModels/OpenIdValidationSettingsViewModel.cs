using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdValidationSettingsViewModel
    {
        [Url]
        public string Authority { get; set; }

        public string Audience { get; set; }
        public string Tenant { get; set; }
        public IEnumerable<string> AvailableTenants { get; set; }
    }
}
