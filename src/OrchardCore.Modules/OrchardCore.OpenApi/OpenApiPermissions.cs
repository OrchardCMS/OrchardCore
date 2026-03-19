using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenApi;

public static class OpenApiPermissions
{
    public static readonly Permission ApiViewContent = new(
        "ApiViewContent",
        "Access view content endpoints"
    );

    public static readonly Permission ApiManage = new(
        "ApiManage",
        "Manage OpenAPI settings and access interactive documentation UIs",
        [ApiViewContent]
    );
}
