using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing
{
    // Remark: 

    public class ContentPickerFieldIndex : ContentFieldIndex
    {
        public string ContentItemId { get; set; }
    }

    public class ContentPickerFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public ContentPickerFieldIndexProvider(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<ContentPickerFieldIndex>()
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
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(ContentPickerField)))
                        .ToArray();

                    var results = new List<ContentPickerFieldIndex>();

                    // Get all field values
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

                        var field = jField.ToObject<ContentPickerField>();

                        foreach (var contentItemId in field.ContentItemIds)
                        {
                            results.Add(new ContentPickerFieldIndex
                            {
                                ContentItemId = contentItemId,
                                ContentType = contentItem.ContentType,
                                ContentPart = fieldDefinition.PartDefinition.Name,
                                ContentField = fieldDefinition.Name
                            });
                        }
                    }

                    return results;
                });
        }
    }
}