using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentsTransfer;

public class ImportPermissions
{
    public static readonly Permission ImportContentFromFile = new("ImportContentFromFile", "Import content items from file");
    public static readonly Permission ImportContentFromFileOfType = new("ImportContentFromFile_{0}", "Import {0} content items from file", new[] { ImportContentFromFile });
                   
    public static readonly Permission ExportContentFromFile = new("ExportContentFromFile", "Export content items from file");
    public static readonly Permission ExportContentFromFileOfType = new("ExportContentFromFile_{0}", "Export {0} content items from file", new[] { ExportContentFromFile });

    private static Dictionary<ValueTuple<string, string>, Permission> _permissionsByType = new ();

    public static Permission CreateDynamicPermission(Permission template, string contentType)
    {
        if (template == null)
        {
            throw new ArgumentNullException(nameof(template));
        }

        var key = new ValueTuple<string, string>(template.Name, contentType);

        if (_permissionsByType.TryGetValue(key, out var permission))
        {
            return permission;
        }

        permission = new Permission(
            string.Format(template.Name, contentType),
            string.Format(template.Description, contentType),
            (template.ImpliedBy ?? Array.Empty<Permission>()).Select(t => CreateDynamicPermission(t, contentType)
            )
        );

        var localPermissions = new Dictionary<ValueTuple<string, string>, Permission>(_permissionsByType)
        {
            [key] = permission
        };
        _permissionsByType = localPermissions;

        return permission;
    }
}
