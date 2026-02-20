using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Data;
using OrchardCore.Html.Fields;
using YesSql.Indexes;

namespace OrchardCore.Html.Indexing.SQL;

public class HtmlFieldIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _ignoredTypes = [];
    private IContentDefinitionManager _contentDefinitionManager;

    public HtmlFieldIndexProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<HtmlFieldIndex>()
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

                // Search for Html fields
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

                // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                if (contentTypeDefinition == null)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                var fieldDefinitions = contentTypeDefinition
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(HtmlField)))
                    .ToArray();

                // This type doesn't have any HtmlField, ignore it
                if (fieldDefinitions.Length == 0)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                return GetHtmlContentField(fieldDefinitions, contentItem)
                    .Select(pair =>
                        new HtmlFieldIndex
                        {
                            Latest = contentItem.Latest,
                            Published = contentItem.Published,
                            ContentItemId = contentItem.ContentItemId,
                            ContentItemVersionId = contentItem.ContentItemVersionId,
                            ContentType = contentItem.ContentType,
                            ContentPart = pair.Definition.ContentTypePartDefinition.Name,
                            ContentField = pair.Definition.Name,
                            Html = pair.Field.Html,
                        });
            });
    }

    private static IEnumerable<(ContentPartFieldDefinition Definition, HtmlField Field)> GetHtmlContentField(
        IEnumerable<ContentPartFieldDefinition> fieldDefinitions,
        ContentItem contentItem)
    {
        foreach (var fieldDefinition in fieldDefinitions)
        {
            var field = GetHtmlContentField(fieldDefinition, contentItem);

            if (field is not null)
            {
                yield return (fieldDefinition, field);
            }
        }
    }

    private static HtmlField GetHtmlContentField(ContentPartFieldDefinition fieldDefinition, ContentItem contentItem)
    {
        if (((JsonObject)contentItem.Content)[fieldDefinition.ContentTypePartDefinition.Name] is not JsonObject jPart ||
            jPart[fieldDefinition.Name] is not JsonObject jField)
        {
            return null;
        }

        return jField.ToObject<HtmlField>();
    }
}
