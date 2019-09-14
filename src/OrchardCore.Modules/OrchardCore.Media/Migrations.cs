using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Fields;
using OrchardCore.Media.Settings;

namespace OrchardCore.Media
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        // This migration does not need to run on new installations, but because there is no
        // initial migration record, there is no way to shortcut the Create migration.
        public int Create()
        {
            _contentDefinitionManager.MigrateFieldSettings<MediaField, MediaFieldSettings>();

            return 1;
        }
    }
}
