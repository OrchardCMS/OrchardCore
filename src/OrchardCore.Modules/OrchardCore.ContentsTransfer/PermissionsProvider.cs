using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentsTransfer;

public class PermissionsProvider : IPermissionProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    private readonly IEnumerable<Permission> _allPermissions =
    [
        ContentTransferPermissions.ListContentTransferEntries,
        ContentTransferPermissions.DeleteContentTransferEntries,
        ContentTransferPermissions.ImportContentFromFile,
        ContentTransferPermissions.ExportContentFromFile,
    ];

    public PermissionsProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>(_allPermissions);

        foreach (var contentTypeDefinition in await _contentDefinitionManager.LoadTypeDefinitionsAsync())
        {
            permissions.Add(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTransferPermissions.ImportContentFromFileOfType, contentTypeDefinition.Name));
            permissions.Add(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTransferPermissions.ExportContentFromFileOfType, contentTypeDefinition.Name));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
    {
        return new[]
        {
            new PermissionStereotype
            {
                Name = OrchardCoreConstants.Roles.Administrator,
                Permissions = _allPermissions,
            },
        };
    }
}
