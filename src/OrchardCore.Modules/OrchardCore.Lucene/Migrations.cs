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
                        if (partDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingPartSettings) && !partDefinition.Settings.TryGetValue(nameof(LuceneContentIndexSettings), out var existingLucenePartSettings))
                        {
                            partDefinition.Settings.Add(new JProperty(nameof(LuceneContentIndexSettings), existingPartSettings));
                        }

                        partDefinition.Settings.Remove("ContentIndexSettings");
                    });
                }
            }

            var partDefinitions = _contentDefinitionManager.LoadPartDefinitions();

            foreach (var partDefinition in partDefinitions)
            {
                if (partDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingPartSettings) && !partDefinition.Settings.TryGetValue(nameof(LuceneContentIndexSettings), out var existingLucenePartSettings))
                {
                    partDefinition.Settings.Add(new JProperty(nameof(LuceneContentIndexSettings), existingPartSettings));
                }

                partDefinition.Settings.Remove("ContentIndexSettings");

                _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                {
                    foreach (var fieldDefinition in partDefinition.Fields)
                    {
                        if (fieldDefinition.Settings.TryGetValue("ContentIndexSettings", out var existingFieldSettings) && !fieldDefinition.Settings.TryGetValue(nameof(LuceneContentIndexSettings), out var existingLuceneFieldSettings))
                        {
                            fieldDefinition.Settings.Add(new JProperty(nameof(LuceneContentIndexSettings), existingFieldSettings));
                        }

                        fieldDefinition.Settings.Remove("ContentIndexSettings");
                    }
                });
            }

            return 1;
        }
    }
}
