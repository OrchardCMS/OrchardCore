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
    public class DateFieldIndex : ContentFieldIndex
    {
        public DateTime? Date { get; set; }
    }

    public class DateFieldIndexProvider : ContentFieldIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new();
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

                    // Search for DateField
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                    if (contentTypeDefinition == null)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    var fieldDefinitions = contentTypeDefinition
                        .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(DateField)))
                        .ToArray();

                    // This type doesn't have any DateField, ignore it
                    if (fieldDefinitions.Length == 0)
                    {
                        _ignoredTypes.Add(contentItem.ContentType);
                        return null;
                    }

                    return fieldDefinitions
                        .GetContentFields<DateField>(contentItem)
                        .Select(pair => new DateFieldIndex
                        {
                            Latest = contentItem.Latest,
                            Published = contentItem.Published,
                            ContentItemId = contentItem.ContentItemId,
                            ContentItemVersionId = contentItem.ContentItemVersionId,
                            ContentType = contentItem.ContentType,
                            ContentPart = pair.Definition.PartDefinition.Name,
                            ContentField = pair.Definition.Name,
                            Date = pair.Field.Value,
                        });
                });
        }
    }
}
