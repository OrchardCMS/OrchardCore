using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenApi;

public static class OpenApiPermissions
{
    public static readonly Permission ManageOpenApi = new(
        "ManageOpenApi",
        LocalizationSource.Create("Manage OpenAPI settings and access interactive documentation UIs", typeof(OpenApiPermissions))
    );

    public static readonly Permission ViewOpenApiContent = new(
        "ViewOpenApiContent",
        LocalizationSource.Create("Access view content endpoints", typeof(OpenApiPermissions)),
        [ManageOpenApi]
    );
}
