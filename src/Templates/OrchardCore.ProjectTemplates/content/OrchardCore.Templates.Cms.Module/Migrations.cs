using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace OrchardCore.Templates.Cms.Module
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public Task<int> CreateAsync()
        {
            await _contentDefinitionManager.AlterPartDefinitionAsync("MyTestPart", builder => builder
                .Attachable()
                .WithDescription("Provides a MyTest part for your content item."));

            return 1;
        }
    }
}
