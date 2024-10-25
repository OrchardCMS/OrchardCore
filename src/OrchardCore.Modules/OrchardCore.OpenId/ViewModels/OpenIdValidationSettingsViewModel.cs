namespace OrchardCore.OpenId.ViewModels;

public class OpenIdValidationSettingsViewModel
{
    [Url]
    public string MetadataAddress { get; set; }

    [Url]
    public string Authority { get; set; }

    public string Audience { get; set; }

    public bool DisableTokenTypeValidation { get; set; }

    public string Tenant { get; set; }
}
