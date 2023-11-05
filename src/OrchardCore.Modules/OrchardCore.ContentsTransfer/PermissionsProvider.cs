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

    public Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>()
        {
            ContentTransferPermissions.ImportContentFromFile,
            ContentTransferPermissions.ExportContentFromFile,
        };

        foreach (var contentTypeDefinition in _contentDefinitionManager.LoadTypeDefinitions())
        {
            permissions.Add(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTransferPermissions.ImportContentFromFileOfType, contentTypeDefinition.Name));
            permissions.Add(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTransferPermissions.ExportContentFromFileOfType, contentTypeDefinition.Name));
        }

        return Task.FromResult<IEnumerable<Permission>>(permissions);
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
                        ContentTransferPermissions.ImportContentFromFile,
                        ContentTransferPermissions.ExportContentFromFile,
                    }
                }
            };
    }
}
