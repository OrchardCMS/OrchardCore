using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenApi;

public static class OpenApiPermissions
{
    public static readonly Permission ManageOpenApi = new(
        "ManageOpenApi",
        LocalizedString.Create("Manage OpenAPI settings and access interactive documentation UIs", typeof(OpenApiPermissions))
    );

    public static readonly Permission ViewOpenApiContent = new(
        "ViewOpenApiContent",
        LocalizedString.Create("Access view content endpoints", typeof(OpenApiPermissions)),
        [ManageOpenApi]
    );
}
