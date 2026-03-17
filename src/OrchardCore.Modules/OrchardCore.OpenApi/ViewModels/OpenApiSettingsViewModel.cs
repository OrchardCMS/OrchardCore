using System.ComponentModel.DataAnnotations;

namespace OrchardCore.OpenApi.ViewModels;

public class OpenApiSettingsViewModel
{
    public bool EnableSwaggerUI { get; set; }

    public bool EnableReDocUI { get; set; }

    public bool EnableScalarUI { get; set; }

    public string AuthorizationUrl { get; set; }

    public string TokenUrl { get; set; }

    public string OAuthClientId { get; set; }

    public string OAuthScopes { get; set; }
}
