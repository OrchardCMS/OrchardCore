using System.Linq;
using OrchardCore.Autoroute.Drivers;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;

namespace OrchardCore.Autoroute
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
            _contentDefinitionManager.AlterPartDefinition("AutoroutePart", builder => builder
                .Attachable()
                .WithDescription("Provides a custom url for your content item."));

            SchemaBuilder.CreateMapIndexTable(nameof(AutoroutePartIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("Path", col => col.WithLength(AutoroutePartDisplay.MaxPathLength))
                .Column<bool>("Published")
            );

            SchemaBuilder.AlterTable(nameof(AutoroutePartIndex), table => table
                .CreateIndex("IDX_AutoroutePartIndex_ContentItemId", "ContentItemId")
            );

            return 1;
        }

        public int UpdateFrom1()
        {
            // TODO Wrap this whole method in an extension on IContentDefinitionManager with TPart, TSettings.
            var contentTypes = _contentDefinitionManager.ListTypeDefinitions();

            foreach (var contentType in contentTypes)
            {
                var partDefinition = contentType.Parts.FirstOrDefault(x => x.PartDefinition.Name == "AutoroutePart");
                if (partDefinition != null)
                {
                    //TODO for others this may need to check if the existing settings have some value.
                    var existingSettings = partDefinition.Settings.ToObject<AutoroutePartSettings>();

                    // Remove existing properties from JObject
                    var properties = typeof(AutoroutePartSettings).GetProperties();
                    foreach (var property in properties)
                    {
                        partDefinition.Settings.Remove(property.Name);
                    }

                    // Apply existing settings to type definition WithSettings<T>
                    _contentDefinitionManager.AlterTypeDefinition(contentType.Name, typeBuilder =>
                    {
                        typeBuilder.WithPart(partDefinition.Name, partBuilder =>
                        {
                            partBuilder.WithSettings(existingSettings);
                        });
                    });
                }
            }

            return 2;
        }
    }
}