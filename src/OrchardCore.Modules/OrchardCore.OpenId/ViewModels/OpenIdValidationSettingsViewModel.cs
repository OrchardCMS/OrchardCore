using System.Collections.Generic;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdValidationSettingsViewModel
    {
        [Url]
        public string MetadataAddress { get; set; }

        [Url]
        public string Authority { get; set; }

        public string Audience { get; set; }
        public bool DisableTokenTypeValidation { get; set; }
        public string Tenant { get; set; }
        public IEnumerable<string> AvailableTenants { get; set; }
    }
}
