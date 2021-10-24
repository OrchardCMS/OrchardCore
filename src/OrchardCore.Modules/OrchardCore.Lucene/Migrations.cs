using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Lucene.Model;

namespace OrchardCore.Lucene
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        // Lucene Settings migration.
        // This code can be removed in a later version.
        public int Create()
        {
            var contentTypeDefinitions = _contentDefinitionManager.LoadTypeDefinitions();
            
            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                foreach (var partDefinition in contentTypeDefinition.Parts)
                {
                    _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                    {
                        LuceneContentIndexSettings newPartSettings = null;

                        if (partDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingPartSettings))
                        {
                            partDefinition.Settings.Remove("ContentIndexSettings");
                            partDefinition.Settings.Remove("LuceneContentIndexSettings");
                            partDefinition.Settings.Add(new JProperty("LuceneContentIndexSettings", existingPartSettings));

                            newPartSettings = partDefinition.Settings.ToObject<LuceneContentIndexSettings>();
                        }

                        if (existingPartSettings != null)
                        {
                            partBuilder.WithSettings(newPartSettings);
                        }
                    });
                }
            }

            var partDefinitions = _contentDefinitionManager.LoadPartDefinitions();

            foreach (var partDefinition in partDefinitions)
            {
                LuceneContentIndexSettings newPartSettings = null;

                if (partDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingPartSettings))
                {
                    partDefinition.Settings.Remove("ContentIndexSettings");
                    partDefinition.Settings.Remove("LuceneContentIndexSettings");
                    partDefinition.Settings.Add(new JProperty("LuceneContentIndexSettings", existingPartSettings));

                    newPartSettings = partDefinition.Settings.ToObject<LuceneContentIndexSettings>();
                }

                _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                {
                    if (existingPartSettings != null)
                    {
                        partBuilder.WithSettings(newPartSettings);
                    }

                    foreach (var fieldDefinition in partDefinition.Fields)
                    {
                        LuceneContentIndexSettings newFieldSettings = null;

                        if (fieldDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingFieldSettings))
                        {
                            fieldDefinition.Settings.Remove("ContentIndexSettings");
                            fieldDefinition.Settings.Remove("LuceneContentIndexSettings");
                            fieldDefinition.Settings.Add(new JProperty("LuceneContentIndexSettings", existingFieldSettings));

                            newFieldSettings = fieldDefinition.Settings.ToObject<LuceneContentIndexSettings>();
                        }

                        if (existingFieldSettings != null)
                        {
                            partBuilder.WithField(fieldDefinition.Name, fieldBuilder =>
                            {
                                fieldBuilder.WithSettings(newFieldSettings);
                            });
                        }
                    }
                });
            }

            return 1;
        }
    }
}
