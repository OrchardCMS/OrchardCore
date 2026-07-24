namespace OrchardCore.OpenApi.ViewModels;

public class OpenApiSettingsViewModel
{
    public bool IsSwaggerUIEnabled { get; set; }

    public bool IsReDocUIEnabled { get; set; }

    public bool IsScalarUIEnabled { get; set; }

    public bool AllowAnonymousSchemaAccess { get; set; }
}
