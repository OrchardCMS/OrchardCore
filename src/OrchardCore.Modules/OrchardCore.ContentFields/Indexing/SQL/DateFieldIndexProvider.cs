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
    public class DateFieldIndex : ContentFieldIndex
    {
        public DateTime? Date { get; set; }
    }

    public class DateFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public DateFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<DateFieldIndex>()
                .Map(contentItem =>
                {
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
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(DateField)))
                        .ToArray();

                    var results = new List<DateFieldIndex>();

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

                        var field = jField.ToObject<DateField>();

                        results.Add(new DateFieldIndex
                        {
                            Latest = contentItem.Latest,
                            Published = contentItem.Published,
                            ContentItemId = contentItem.ContentItemId,
                            ContentItemVersionId = contentItem.ContentItemVersionId,
                            ContentType = contentItem.ContentType,
                            ContentPart = fieldDefinition.PartDefinition.Name,
                            ContentField = fieldDefinition.Name,
                            Date = field.Value
                        });
                    }

                    return results;
                });
        }
    }
}
