using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents;

public static class ContentTypesPermissions
{
    public static readonly Permission ViewContentTypes = new("ViewContentTypes", LocalizationSource.Create("View content types.", typeof(ContentTypesPermissions)));

    public static readonly Permission EditContentTypes = new("EditContentTypes", LocalizationSource.Create("Edit content types.", typeof(ContentTypesPermissions)), isSecurityCritical: true);
}
