namespace OrchardCore.OpenApi.Settings;

public class OpenApiSettings
{
    /// <summary>
    /// Whether the OpenAPI JSON schema endpoints can be accessed without authentication.
    /// When <c>false</c> (the default), the schema endpoints require the
    /// <c>ViewOpenApiContent</c> permission.
    /// </summary>
    public bool AllowAnonymousSchemaAccess { get; set; }
}
