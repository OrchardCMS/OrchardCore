using System;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.ContentManagement.Records
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(ContentItemIndex), table => table
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<string>("ContentItemVersionId", c => c.WithLength(26))
                .Column<bool>("Latest")
                .Column<bool>("Published")
                .Column<string>("ContentType", column => column.WithLength(ContentItemIndex.MaxContentTypeSize))
                .Column<DateTime>("ModifiedUtc", column => column.Nullable())
                .Column<DateTime>("PublishedUtc", column => column.Nullable())
                .Column<DateTime>("CreatedUtc", column => column.Nullable())
                .Column<string>("Owner", column => column.Nullable().WithLength(ContentItemIndex.MaxOwnerSize))
                .Column<string>("Author", column => column.Nullable().WithLength(ContentItemIndex.MaxAuthorSize))
                .Column<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemId", "ContentItemId", "Latest", "Published", "CreatedUtc")
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_DisplayText", "DisplayText")
            );

            return 3;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .AddColumn<string>("ContentItemVersionId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_ContentItemVersionId", "ContentItemVersionId")
            );

            return 2;
        }

        public int UpdateFrom2()
        {
            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .AddColumn<string>("DisplayText", column => column.Nullable().WithLength(ContentItemIndex.MaxDisplayTextSize))
            );

            SchemaBuilder.AlterTable(nameof(ContentItemIndex), table => table
                .CreateIndex("IDX_ContentItemIndex_DisplayText", "DisplayText")
            );

            // Return 3 to shortcut the 3rd and 4th migration on new content definition schemas.
            return 5;
        }

        // Migrate content type definitions. This only needs to run on old content definition schemas.
        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            var contentTypeDefinitions = _contentDefinitionManager.LoadTypeDefinitions();
            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                var existingContentTypeSettings = contentTypeDefinition.Settings.ToObject<ContentTypeSettings>();

                // Do this before creating builder, so settings are removed from the builder settings object.
                // Remove existing properties from JObject
                var contentTypeSettingsProperties = existingContentTypeSettings.GetType().GetProperties();
                foreach (var property in contentTypeSettingsProperties)
                {
                    contentTypeDefinition.Settings.Remove(property.Name);
                }

                _contentDefinitionManager.AlterTypeDefinition(contentTypeDefinition.Name, builder =>
                {
                    builder.WithSettings(existingContentTypeSettings);

                    foreach (var contentTypePartDefinition in contentTypeDefinition.Parts)
                    {
                        var existingTypePartSettings = contentTypePartDefinition.Settings.ToObject<ContentTypePartSettings>();

                        // Remove existing properties from JObject
                        var contentTypePartSettingsProperties = existingTypePartSettings.GetType().GetProperties();
                        foreach (var property in contentTypePartSettingsProperties)
                        {
                            contentTypePartDefinition.Settings.Remove(property.Name);
                        }

                        builder.WithPart(contentTypePartDefinition.Name, contentTypePartDefinition.PartDefinition, partBuilder =>
                        {
                            partBuilder.WithSettings(existingTypePartSettings);
                        });
                    }
                });
            }

            return 4;
        }

        // Migration content part definitions.
        // This code can be removed in a later version.
        public int UpdateFrom4()
        {
            var partDefinitions = _contentDefinitionManager.LoadPartDefinitions();
            foreach (var partDefinition in partDefinitions)
            {
                var existingPartSettings = partDefinition.Settings.ToObject<ContentPartSettings>();

                // Do this before creating builder, so settings are removed from the builder settings object.
                // Remove existing properties from JObject
                var contentTypeSettingsProperties = existingPartSettings.GetType().GetProperties();
                foreach (var property in contentTypeSettingsProperties)
                {
                    partDefinition.Settings.Remove(property.Name);
                }

                _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                {
                    partBuilder.WithSettings(existingPartSettings);
                    foreach (var fieldDefinition in partDefinition.Fields)
                    {
                        var existingFieldSettings = fieldDefinition.Settings.ToObject<ContentPartFieldSettings>();

                        // Do this before creating builder, so settings are removed from the builder settings object.
                        // Remove existing properties from JObject
                        var fieldSettingsProperties = existingFieldSettings.GetType().GetProperties();
                        foreach (var property in fieldSettingsProperties)
                        {
                            fieldDefinition.Settings.Remove(property.Name);
                        }

                        partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                        {
                            fieldBuilder.WithSettings(existingFieldSettings);
                        });
                    }
                });
            }

            return 5;
        }
    }
}
