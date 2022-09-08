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
    public class TextFieldIndex : ContentFieldIndex
    {
        // Maximum length that MySql can support in an index under utf8 collation is 768,
        // minus 1 for the `DocumentId` integer (character size = integer size = 4 bytes).
        // minus 1 (freeing 4 bytes) for the additional 'Published' and 'Latest' booleans.
        public const int MaxTextSize = 766;

        public string Text { get; set; }
        public string BigText { get; set; }
    }

    public class TextFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new();
        private IContentDefinitionManager _contentDefinitionManager;

        public TextFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<TextFieldIndex>()
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
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TextField)))
                        .ToArray();

                    // This type doesn't have any TextField, ignore it
                    if (fieldDefinitions.Length == 0)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    return fieldDefinitions
                        .GetContentFields<TextField>(contentItem)
                        .Select(pair =>
                            new TextFieldIndex
                            {
                                Latest = contentItem.Latest,
                                Published = contentItem.Published,
                                ContentItemId = contentItem.ContentItemId,
                                ContentItemVersionId = contentItem.ContentItemVersionId,
                                ContentType = contentItem.ContentType,
                                ContentPart = pair.Definition.PartDefinition.Name,
                                ContentField = pair.Definition.Name,
                                Text = pair.Field.Text?[..Math.Min(pair.Field.Text.Length, TextFieldIndex.MaxTextSize)],
                                BigText = pair.Field.Text,
                            });
                });
        }
    }
}
