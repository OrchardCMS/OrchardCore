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
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
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
                    // Can we safely ignore this content item?
                    if (_ignoredTypes.Contains(contentItem.ContentType))
                    {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();

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

                    var results = new List<TextFieldIndex>();

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

                        var field = jField.ToObject<TextField>();

                        results.Add(new TextFieldIndex
                        {
                            Latest = contentItem.Latest,
                            Published = contentItem.Published,
                            ContentItemId = contentItem.ContentItemId,
                            ContentItemVersionId = contentItem.ContentItemVersionId,
                            ContentType = contentItem.ContentType,
                            ContentPart = fieldDefinition.PartDefinition.Name,
                            ContentField = fieldDefinition.Name,
                            Text = field.Text?.Substring(0, Math.Min(field.Text.Length, TextFieldIndex.MaxTextSize)),
                            BigText = field.Text
                        });
                    }

                    return results;
                });
        }
    }
}
