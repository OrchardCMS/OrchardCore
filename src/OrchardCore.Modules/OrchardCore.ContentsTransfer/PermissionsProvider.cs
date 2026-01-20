using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.ContentsTransfer;

public class PermissionsProvider : IPermissionProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public PermissionsProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            ContentTransferPermissions.ListContentTransferEntries,
            ContentTransferPermissions.DeleteContentTransferEntries,
            ContentTransferPermissions.ImportContentFromFile,
            ContentTransferPermissions.ExportContentFromFile,
        };

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
                    Name = "Administrator",
                    Permissions = new[]
                    {
                        ContentTransferPermissions.ListContentTransferEntries,
                        ContentTransferPermissions.DeleteContentTransferEntries,
                        ContentTransferPermissions.ImportContentFromFile,
                        ContentTransferPermissions.ExportContentFromFile,
                    }
                }
            };
    }
}
