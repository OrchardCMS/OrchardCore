using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing.SQL;

public class DateTimeFieldIndex : ContentFieldIndex
{
    public DateTime? DateTime { get; set; }
}

public class DateTimeFieldIndexProvider : ContentFieldIndexProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _ignoredTypes = [];
    private IContentDefinitionManager _contentDefinitionManager;

    public DateTimeFieldIndexProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<DateTimeFieldIndex>()
            .Map(async contentItem =>
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

                // Search for DateTimeField
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

                // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                if (contentTypeDefinition == null)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                var fieldDefinitions = contentTypeDefinition
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(DateTimeField)))
                    .ToArray();

                // This type doesn't have any DateTimeField, ignore it
                if (fieldDefinitions.Length == 0)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                return fieldDefinitions
                    .GetContentFields<DateTimeField>(contentItem)
                    .Select(pair =>
                        new DateTimeFieldIndex
                        {
                            Latest = contentItem.Latest,
                            Published = contentItem.Published,
                            ContentItemId = contentItem.ContentItemId,
                            ContentItemVersionId = contentItem.ContentItemVersionId,
                            ContentType = contentItem.ContentType,
                            ContentPart = pair.Definition.ContentTypePartDefinition.Name,
                            ContentField = pair.Definition.Name,
                            DateTime = pair.Field.Value,
                        });
            });
    }
}
