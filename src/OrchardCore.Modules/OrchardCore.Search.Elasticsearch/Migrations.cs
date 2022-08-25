using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.Search.Elasticsearch.Core.Models;

namespace OrchardCore.Search.Elasticsearch
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
            var contentTypeDefinitions = _contentDefinitionManager.LoadTypeDefinitions();

            foreach (var contentTypeDefinition in contentTypeDefinitions)
            {
                foreach (var partDefinition in contentTypeDefinition.Parts)
                {
                    _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                    {
                        if (partDefinition.Settings.TryGetValue("LuceneContentIndexSettings", out var existingPartSettings))
                        {
                            var included = existingPartSettings["Included"];

                            if (included != null && (bool)included)
                            {
                                partDefinition.Settings.Add(new JProperty(nameof(ElasticContentIndexSettings), JToken.FromObject(existingPartSettings.ToObject<ElasticContentIndexSettings>())));
                            }
                            
                        }
                    });
                }
            }

            var partDefinitions = _contentDefinitionManager.LoadPartDefinitions();

            foreach (var partDefinition in partDefinitions)
            {
                _contentDefinitionManager.AlterPartDefinition(partDefinition.Name, partBuilder =>
                {
                    if (partDefinition.Settings.TryGetValue("LuceneContentIndexSettings", out var existingPartSettings))
                    {
                        var included = existingPartSettings["Included"];

                        if (included != null && (bool)included)
                        {
                            partDefinition.Settings.Add(new JProperty(nameof(ElasticContentIndexSettings), JToken.FromObject(existingPartSettings.ToObject<ElasticContentIndexSettings>())));
                        }
                    }

                    foreach (var fieldDefinition in partDefinition.Fields)
                    {
                        if (fieldDefinition.Settings.TryGetValue("LuceneContentIndexSettings", out var existingFieldSettings))
                        {
                            var included = existingFieldSettings["Included"];

                            if (included != null && (bool)included)
                            {
                                fieldDefinition.Settings.Add(new JProperty(nameof(ElasticContentIndexSettings), JToken.FromObject(existingFieldSettings.ToObject<ElasticContentIndexSettings>())));
                            }
                        }
                    }
                });
            }

            return 1;
        }
    }
}
