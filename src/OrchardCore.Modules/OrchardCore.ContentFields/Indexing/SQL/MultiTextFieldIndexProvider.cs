using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing.SQL
{
    public class MultiTextFieldIndex : ContentFieldIndex
    {
        // Maximum length that MySql can support in an index under utf8 collation is 768,
        // minus 1 for the `DocumentId` integer (character size = integer size = 4 bytes).
        // minus 1 (freeing 4 bytes) for the additional 'Published' and 'Latest' booleans.
        public const int MaxValueSize = 766;

        public string Value { get; set; }
        public string BigValue { get; set; }
    }

    public class MultiTextFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new();
        private IContentDefinitionManager _contentDefinitionManager;

        public MultiTextFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<MultiTextFieldIndex>()
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

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for TextField
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                    if (contentTypeDefinition == null)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    var fieldDefinitions = contentTypeDefinition
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(MultiTextField)))
                        .ToArray();

                    // This type doesn't have any MultiTextField, ignore it
                    if (fieldDefinitions.Length == 0)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    return fieldDefinitions
                        .GetContentFields<MultiTextField>(contentItem)
                        .SelectMany(pair =>
                            pair.Field.Values.Select(value => (pair.Definition, Value: value)))
                        .Select(pair =>
                            new MultiTextFieldIndex
                            {
                                Latest = contentItem.Latest,
                                Published = contentItem.Published,
                                ContentItemId = contentItem.ContentItemId,
                                ContentItemVersionId = contentItem.ContentItemVersionId,
                                ContentType = contentItem.ContentType,
                                ContentPart = pair.Definition.PartDefinition.Name,
                                ContentField = pair.Definition.Name,
                                Value = pair.Value?[..Math.Min(pair.Value.Length, MultiTextFieldIndex.MaxValueSize)],
                                BigValue = pair.Value,
                            });
                });
        }
    }
}
