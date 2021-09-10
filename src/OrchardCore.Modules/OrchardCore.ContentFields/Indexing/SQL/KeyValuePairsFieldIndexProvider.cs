using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing.SQL
{
    public class KeyValuePairsFieldIndex : ContentFieldIndex
    {
        // Maximum length that MySql can support in an index under utf8 collation is 768,
        // minus 1 for the `DocumentId` integer (character size = integer size = 4 bytes).
        // minus 1 (freeing 4 bytes) for the additional 'Published' and 'Latest' booleans.
        public const int MaxKeySize = 766;
        public const int MaxValueSize = 766;

        public string Key { get; set; }
        public string Value { get; set; }
        public string BigValue { get; set; }
    }

    public class KeyValuePairsFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public KeyValuePairsFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<KeyValuePairsFieldIndex>()
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

                    // Can we safely ignore this content item?
                    if (_ignoredTypes.Contains(contentItem.ContentType))
                    {
                        return null;
                    }

                    if (!contentItem.Latest && !contentItem.Published)
                    {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for LinkField
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                    if (contentTypeDefinition == null)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    var fieldDefinitions = contentTypeDefinition
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(LinkField)))
                        .ToArray();

                    // This type doesn't have any LinkField, ignore it
                    if (fieldDefinitions.Length == 0)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    var results = new List<KeyValuePairsFieldIndex>();

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

                        var field = jField.ToObject<KeyValuePairsField>();

                        foreach (var value in field.Values)
                        {
                            results.Add(new KeyValuePairsFieldIndex
                            {
                                Latest = contentItem.Latest,
                                Published = contentItem.Published,
                                ContentItemId = contentItem.ContentItemId,
                                ContentItemVersionId = contentItem.ContentItemVersionId,
                                ContentType = contentItem.ContentType,
                                ContentPart = fieldDefinition.PartDefinition.Name,
                                ContentField = fieldDefinition.Name,
                                Key = value.Key?.Substring(0, Math.Min(value.Key.Length, KeyValuePairsFieldIndex.MaxKeySize)),
                                Value = value.Value?.Substring(0, Math.Min(value.Value.Length, KeyValuePairsFieldIndex.MaxValueSize)),
                                BigValue = value.Value
                            });
                        }
                    }

                    return results;
                });
        }
    }
}
