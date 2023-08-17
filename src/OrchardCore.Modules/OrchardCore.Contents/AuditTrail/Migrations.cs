using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;

namespace OrchardCore.Contents.AuditTrail
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(AuditTrailPart), part => part
                .Attachable()
                .WithDescription("Allows editors to enter a comment to be saved into the Audit Trail event when saving a content item."));

            return 1;
        }
    }
}
