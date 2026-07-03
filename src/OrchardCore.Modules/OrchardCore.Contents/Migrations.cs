using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.AdminNodes;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Contents;

public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("CommonPart", builder => builder
            .Attachable()
            .WithDescription("Provides an editor for the common properties of a content item."));

        return 1;
    }

    //migrate admin menu to 3.0 format
    public async Task<int> UpdateFrom1Async()
    {
        // Data manipulation must be deferred so that it runs in a fresh scope after the
        // migration's schema transaction has been committed. Doing it inline in the migration
        // method would not reliably persist the changes made through the document store.
        ShellScope.AddDeferredTask(async scope => await AdminMenuItemMigrator.MigrateItemTo(scope, async (menu, menuItem, scope) =>
                    {
                        var contentTypeMenuItem = menuItem as ContentTypesAdminNode;
                        if (contentTypeMenuItem != null)
                        {
                            foreach (var typeEntry in contentTypeMenuItem.ContentTypes)
                            {
                                if (!string.IsNullOrEmpty(typeEntry.ContentTypeId) && string.IsNullOrEmpty(typeEntry.ContentTypeName))
                                {
                                    var typedef = await _contentDefinitionManager.GetTypeDefinitionAsync(typeEntry.ContentTypeId);
                                    typeEntry.ContentTypeName = typeEntry.ContentTypeId;
                                    typeEntry.ContentTypeDisplayName = typedef.DisplayName;
                                }
                            }
                        }
                    }));

        return 2;
    }
}
