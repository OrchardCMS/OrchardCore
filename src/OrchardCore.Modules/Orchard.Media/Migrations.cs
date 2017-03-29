using Orchard.ContentManagement.Metadata.Settings;
using Orchard.ContentManagement.MetaData;
using Orchard.Data.Migration;
using Orchard.Media.Indexes;

namespace Orchard.Media
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
            _contentDefinitionManager.AlterTypeDefinition("Image", builder => builder
                .Creatable()
                .Listable(false)
                .Draftable(false)
                .WithPart("TitlePart")
                .WithPart("ImagePart")
                .Stereotype("Media"));

            SchemaBuilder.CreateMapIndexTable(nameof(MediaPartIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("MimeType", c => c.WithLength(64))
                .Column<string>("Folder", col => col.WithLength(1024))
                .Column<string>("FileName", col => col.WithLength(1024))
                .Column<long>("Length", col => col.WithLength(1024))
            );

            SchemaBuilder.AlterTable(nameof(MediaPartIndex), table => table
                .CreateIndex("IDX_MediaPartIndex_Folder", "Folder")
            );

            SchemaBuilder.AlterTable(nameof(MediaPartIndex), table => table
                .CreateIndex("IDX_MediaPartIndex_Folder_FileName", new[] { "Folder", "FileName" })
            );

            return 1;
        }
    }
}