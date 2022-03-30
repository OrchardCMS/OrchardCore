using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Media.Settings;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Seo
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IRecipeMigrator _recipeMigrator;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IRecipeMigrator recipeMigrator)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _recipeMigrator = recipeMigrator;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("SeoMetaPart", builder => builder
                .Attachable()
                .WithDescription("Provides a part that allows SEO meta descriptions to be applied to a content item.")
                .WithField("DefaultSocialImage", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Default social image")
                    .WithSettings(new MediaFieldSettings { Multiple = false }))
                .WithField("OpenGraphImage", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Open graph image")
                    .WithSettings(new MediaFieldSettings { Multiple = false }))
                .WithField("TwitterImage", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Twitter image")
                    .WithSettings(new MediaFieldSettings { Multiple = false }))
            );

            return 1;
        }

        public async Task<int> UpdateFrom1Async()
        {
            await _recipeMigrator.ExecuteAsync("socialmetasettings.recipe.json", this);

            return 2;
        }
    }
}
