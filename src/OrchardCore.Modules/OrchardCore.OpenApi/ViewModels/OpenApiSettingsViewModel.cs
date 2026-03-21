using OrchardCore.OpenApi.Settings;

namespace OrchardCore.OpenApi.ViewModels;

public class OpenApiSettingsViewModel
{
    public bool IsSwaggerUIEnabled { get; set; }

    public bool IsReDocUIEnabled { get; set; }

    public bool IsScalarUIEnabled { get; set; }

    public bool AllowAnonymousSchemaAccess { get; set; }

    public OpenApiAuthenticationType AuthenticationType { get; set; }

    public string AuthorizationUrl { get; set; }

    public string TokenUrl { get; set; }

    public string OAuthClientId { get; set; }

    public string OAuthScopes { get; set; }
}
