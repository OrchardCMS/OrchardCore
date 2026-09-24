using Microsoft.Extensions.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Contents;

public static class ContentTypesPermissions
{
    public static readonly Permission ViewContentTypes = new("ViewContentTypes", LocalizedString.Create("View content types.", typeof(ContentTypesPermissions)));

    public static readonly Permission EditContentTypes = new("EditContentTypes", LocalizedString.Create("Edit content types.", typeof(ContentTypesPermissions)), isSecurityCritical: true);
}
