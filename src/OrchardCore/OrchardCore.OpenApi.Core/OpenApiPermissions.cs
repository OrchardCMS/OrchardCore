using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenApi;

public static class OpenApiPermissions
{
    public static readonly Permission ManageOpenApi = new(
        "ManageOpenApi",
        "Manage OpenAPI settings and access interactive documentation UIs"
    );

    public static readonly Permission ViewOpenApiContent = new(
        "ViewOpenApiContent",
        "Access view content endpoints",
        [ManageOpenApi]
    );
}
