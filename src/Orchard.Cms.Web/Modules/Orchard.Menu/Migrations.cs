using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;

namespace Orchard.Menu
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
                .Creatable()
                .WithPart("MenuPart")
                .WithPart("MenuItemsListPart")
            );

            _contentDefinitionManager.AlterTypeDefinition("LinkMenuItem", menu => menu
                .WithPart("LinkMenuItemPart")
                .Stereotype("MenuItem")
            );

            return 1;
        }
    }
}