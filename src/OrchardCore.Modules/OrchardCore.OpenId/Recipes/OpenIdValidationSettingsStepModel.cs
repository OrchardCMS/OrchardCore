namespace OrchardCore.OpenId.Recipes
{
    public class OpenIdValidationSettingsStepModel
    {

        public string MetadataAddress { get; set; }
        public string Audience { get; set; }

        public string Authority { get; set; }
        public bool DisableTokenTypeValidation { get; set; }

        public string Tenant { get; set; }
    }
}
