using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing
{
    // Remark: 

    public class BooleanFieldIndex : MapIndex
    {
        public string FieldName { get; set; }
        public bool? Boolean { get; set; }
    }

    public class BooleanFieldIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public BooleanFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<BooleanFieldIndex>()
                .Map(contentItem =>
                {
                    if (!contentItem.IsPublished())
                    {
                        return null;
                    }

                    // Can we safely ignore this content item?
                    if (_ignoredTypes.Contains(contentItem.ContentType))
                    {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for Text fields
                    var fieldDefinitions = _contentDefinitionManager
                        .GetTypeDefinition(contentItem.ContentType)
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(BooleanField)))
                        .ToArray();

                    var results = new List<BooleanFieldIndex>();

                    foreach (var fieldDefinition in fieldDefinitions)
                    {
                        var jPart = (JObject)contentItem.Content[fieldDefinition.PartDefinition.Name];

                        if (jPart == null)
                        {
                            continue;
                        }

                        var jField = (JObject)jPart[fieldDefinition.Name];

                        if (jField == null)
                        {
                            continue;
                        }

                        var field = jField.ToObject<BooleanField>();

                        results.Add(new BooleanFieldIndex
                        {
                            FieldName = fieldDefinition.Name,
                            Boolean = field.Value
                        });
                    }

                    return results;
                });
        }


    }
}