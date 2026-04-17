using OrchardCore.Security.Permissions;

namespace OrchardCore.OpenApi;

public static class OpenApiPermissions
{
    public static readonly Permission ApiViewContent = new(
        "ApiViewContent",
        "Access view content endpoints"
    );
}
