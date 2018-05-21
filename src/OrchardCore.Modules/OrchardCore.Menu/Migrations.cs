using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.Menu
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Menu", menu => menu
                .Draftable()
                .Versionable()
                .Creatable()
                .Listable()
                .WithPart("TitlePart", part => part.WithPosition("1"))
                .WithPart("AliasPart", part => part.WithPosition("2").WithSettings(new AliasPartSettings { Pattern = "{{ ContentItem | display_text | slugify }}" }))
                .WithPart("MenuPart", part => part.WithPosition("3"))
                .WithPart("MenuItemsListPart", part => part.WithPosition("4"))
            );

            _contentDefinitionManager.AlterTypeDefinition("LinkMenuItem", menu => menu
                .WithPart("LinkMenuItemPart")
                .Stereotype("MenuItem")
            );

            return 1;
        }
    }

    class AliasPartSettings
    {
        public string Pattern { get; set; }
    }
}
