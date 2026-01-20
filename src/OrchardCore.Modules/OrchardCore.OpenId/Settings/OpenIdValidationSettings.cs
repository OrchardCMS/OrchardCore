using System;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdValidationSettings
    {
        public string Audience { get; set; }
        public Uri Authority { get; set; }
        public bool DisableTokenTypeValidation { get; set; }
        public string Tenant { get; set; }
        public Uri MetadataAddress { get; set; }

    }
}
