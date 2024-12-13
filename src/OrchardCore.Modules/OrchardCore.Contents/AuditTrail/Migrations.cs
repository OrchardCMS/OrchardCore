using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace OrchardCore.Contents.AuditTrail;

[RequireFeatures("OrchardCore.AuditTrail")]
public sealed class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("AuditTrailPart", part => part
            .Attachable()
            .WithDescription("Allows editors to enter a comment to be saved into the Audit Trail event when saving a content item."));

        return 1;
    }
}
