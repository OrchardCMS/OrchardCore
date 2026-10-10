using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using YesSql;
using YesSql.Services;

namespace OrchardCore.DataSources.Contents;

/// <summary>
/// Exposes each content type as a data set. Rows are read page by page in document order, with the common columns of
/// every content item followed by the values of its parts and fields that <see cref="ContentDataSourceOptions"/>
/// describes. A user sees the content types they may view. A user who may also edit a content type reads the latest
/// version of its items, drafts included; any other user reads the published versions only.
/// </summary>
public sealed class ContentItemsDataSource : IDataSource
{
    /// <summary>
    /// The technical name of the data source.
    /// </summary>
    public const string SourceName = "Contents";

    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly ContentDataSourceOptions _options;
    private readonly IStringLocalizer S;

    public ContentItemsDataSource(
        IContentDefinitionManager contentDefinitionManager,
        ISession session,
        IAuthorizationService authorizationService,
        IOptions<ContentDataSourceOptions> options,
        IStringLocalizer<ContentItemsDataSource> localizer)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _session = session;
        _authorizationService = authorizationService;
        _options = options.Value;
        S = localizer;
    }

    public string Name => SourceName;

    public LocalizedString DisplayName => S["Content items"];

    public LocalizedString Description => S["The content items of each content type, with their parts and fields."];

    public async Task<IReadOnlyList<DataSetDescriptor>> GetDataSetsAsync(DataSourceContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var result = new List<DataSetDescriptor>();

        foreach (var definition in await _contentDefinitionManager.ListTypeDefinitionsAsync())
        {
            if (await CanReadAsync(context.User, definition))
            {
                result.Add(Describe(definition));
            }
        }

        return result
            .OrderBy(dataSet => dataSet.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<DataSetSchema> GetSchemaAsync(string dataSet, DataSourceContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        var definition = await GetReadableDefinitionAsync(dataSet, context.User);

        if (definition is null)
        {
            return null;
        }

        return new DataSetSchema
        {
            DataSet = Describe(definition),
            Fields = BuildColumns(definition).Select(column => column.Field).ToList(),
        };
    }

    public async IAsyncEnumerable<DataBatch> ReadAsync(DataSourceQuery query, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var definition = await GetReadableDefinitionAsync(query.DataSet, query.Context?.User);

        if (definition is null)
        {
            yield break;
        }

        var columns = BuildColumns(definition);

        if (query.Fields?.Count > 0)
        {
            columns = columns.Where(column => query.Fields.Contains(column.Field.Name)).ToList();
        }

        var fields = columns.Select(column => column.Field).ToArray();
        var includeDrafts = await _authorizationService.AuthorizeContentTypeAsync(query.Context.User, CommonPermissions.EditContent, definition);
        var batchSize = Math.Max(1, query.BatchSize);
        var remaining = query.MaxRows > 0 ? query.MaxRows : long.MaxValue;
        var lastDocumentId = 0L;

        while (remaining > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var take = (int)Math.Min(batchSize, remaining);
            var contentType = definition.Name;
            var after = lastDocumentId;

            var itemsQuery = includeDrafts
                ? _session.Query<ContentItem, ContentItemIndex>(index => index.ContentType == contentType && index.Latest && index.DocumentId > after)
                : _session.Query<ContentItem, ContentItemIndex>(index => index.ContentType == contentType && index.Published && index.DocumentId > after);
            itemsQuery = ApplyConditions(itemsQuery, query.Conditions);

            var items = (await itemsQuery.OrderBy(index => index.DocumentId).Take(take).ListAsync(cancellationToken)).ToList();

            if (items.Count == 0)
            {
                yield break;
            }

            var rows = new List<object[]>(items.Count);

            foreach (var item in items)
            {
                var row = new object[columns.Count];

                for (var index = 0; index < columns.Count; index++)
                {
                    row[index] = columns[index].Read(item);
                }

                rows.Add(row);
            }

            lastDocumentId = items[^1].Id;
            remaining -= items.Count;

            // Free the identity map, so reading a large content type doesn't keep every item in memory.
            _session.Detach(items);

            yield return new DataBatch(fields, rows);

            if (items.Count < take)
            {
                yield break;
            }
        }
    }

    private static IQuery<ContentItem, ContentItemIndex> ApplyConditions(IQuery<ContentItem, ContentItemIndex> query, IEnumerable<DataCondition> conditions)
    {
        foreach (var condition in conditions ?? [])
        {
            var values = condition.Values ?? [];
            var first = values.FirstOrDefault();

            switch (condition.Field)
            {
                case "Published" when condition.Operator == DataFilterOperator.Equals && first is bool published:
                    query = query.Where(index => index.Published == published);
                    break;

                case "ContentItemId" or "Owner" when condition.Operator is DataFilterOperator.In or DataFilterOperator.Equals:
                    var keys = values.OfType<string>().ToArray();

                    if (keys.Length == 0)
                    {
                        break;
                    }

                    query = condition.Field == "Owner"
                        ? query.Where(index => index.Owner.IsIn(keys))
                        : query.Where(index => index.ContentItemId.IsIn(keys));
                    break;

                case "CreatedUtc" or "ModifiedUtc" or "PublishedUtc":
                    query = ApplyDateCondition(query, condition);
                    break;
            }
        }

        return query;
    }

    private static IQuery<ContentItem, ContentItemIndex> ApplyDateCondition(IQuery<ContentItem, ContentItemIndex> query, DataCondition condition)
    {
        var values = condition.Values ?? [];
        var from = values.Count > 0 ? values[0] as DateTime? : null;
        var to = values.Count > 1 ? values[1] as DateTime? : null;

        (DateTime? Lower, DateTime? Upper) range = condition.Operator switch
        {
            DataFilterOperator.GreaterThan or DataFilterOperator.GreaterThanOrEqual => (from, null),
            DataFilterOperator.LessThan or DataFilterOperator.LessThanOrEqual => (null, from),
            DataFilterOperator.Between => (from, to),
            _ => (null, null),
        };

        // The bounds are inclusive, so a strict comparison keeps a few more rows, which the consumer filters again.
        if (range.Lower is { } lower)
        {
            query = condition.Field switch
            {
                "CreatedUtc" => query.Where(index => index.CreatedUtc >= lower),
                "ModifiedUtc" => query.Where(index => index.ModifiedUtc >= lower),
                _ => query.Where(index => index.PublishedUtc >= lower),
            };
        }

        if (range.Upper is { } upper)
        {
            query = condition.Field switch
            {
                "CreatedUtc" => query.Where(index => index.CreatedUtc <= upper),
                "ModifiedUtc" => query.Where(index => index.ModifiedUtc <= upper),
                _ => query.Where(index => index.PublishedUtc <= upper),
            };
        }

        return query;
    }

    private async Task<ContentTypeDefinition> GetReadableDefinitionAsync(string dataSet, ClaimsPrincipal user)
    {
        if (string.IsNullOrEmpty(dataSet))
        {
            return null;
        }

        var definition = await _contentDefinitionManager.GetTypeDefinitionAsync(dataSet);

        return definition is not null && await CanReadAsync(user, definition) ? definition : null;
    }

    private async Task<bool> CanReadAsync(ClaimsPrincipal user, ContentTypeDefinition definition)
        => user is not null && await _authorizationService.AuthorizeContentTypeAsync(user, CommonPermissions.ViewContent, definition);

    private DataSetDescriptor Describe(ContentTypeDefinition definition)
        => new(definition.Name, definition.DisplayName)
        {
            Group = definition.GetStereotype() is { Length: > 0 } stereotype ? stereotype : S["Content"].Value,
            DefaultDateField = "CreatedUtc",
        };

    private List<ContentColumn> BuildColumns(ContentTypeDefinition definition)
    {
        var common = S["Content item"].Value;
        var columns = new List<ContentColumn>
        {
            new(new DataField("ContentItemId", S["Content item id"], DataFieldType.Text, common) { IsIdentifier = true, IsKeyFilterable = true }, item => item.ContentItemId),
            new(new DataField("ContentItemVersionId", S["Version id"], DataFieldType.Text, common) { IsIdentifier = true }, item => item.ContentItemVersionId),
            new(new DataField("ContentType", S["Content type"], DataFieldType.Text, common), item => item.ContentType),
            new(new DataField("DisplayText", S["Display text"], DataFieldType.Text, common), item => item.DisplayText),
            new(new DataField("Owner", S["Owner"], DataFieldType.Text, common)
            {
                IsIdentifier = true,
                IsKeyFilterable = true,
                References = [new("Users", "Users", "UserId")],
            }, item => item.Owner),
            new(new DataField("Author", S["Author"], DataFieldType.Text, common), item => item.Author),
            new(new DataField("Published", S["Published"], DataFieldType.Boolean, common), item => item.Published),
            new(new DataField("Latest", S["Latest"], DataFieldType.Boolean, common), item => item.Latest),
            new(new DataField("CreatedUtc", S["Created"], DataFieldType.DateTime, common), item => item.CreatedUtc),
            new(new DataField("ModifiedUtc", S["Modified"], DataFieldType.DateTime, common), item => item.ModifiedUtc),
            new(new DataField("PublishedUtc", S["Published on"], DataFieldType.DateTime, common), item => item.PublishedUtc),
        };

        foreach (var typePart in definition.Parts)
        {
            var partName = typePart.Name;
            var partLabel = typePart.DisplayName();

            if (_options.Parts.TryGetValue(typePart.PartDefinition.Name, out var partValues))
            {
                foreach (var value in partValues)
                {
                    columns.Add(new(
                        new DataField($"{partName}.{value.Property}", $"{partLabel} {value.Property}", value.Type, partLabel),
                        item => Read(((JsonObject)item.Content)[partName]?[value.Property], value)));
                }
            }

            foreach (var partField in typePart.PartDefinition.Fields)
            {
                if (!_options.Fields.TryGetValue(partField.FieldDefinition.Name, out var fieldValues))
                {
                    continue;
                }

                var fieldName = partField.Name;
                var fieldLabel = partField.DisplayName();

                foreach (var value in fieldValues)
                {
                    var name = fieldValues.Length == 1 ? $"{partName}.{fieldName}" : $"{partName}.{fieldName}.{value.Property}";
                    var label = fieldValues.Length == 1 ? fieldLabel : $"{fieldLabel} {value.Property}";

                    columns.Add(new(
                        new DataField(name, label, value.Type, partLabel),
                        item => Read(((JsonObject)item.Content)[partName]?[fieldName]?[value.Property], value)));
                }
            }
        }

        return columns;
    }

    private static object Read(JsonNode node, ContentDataValue value)
    {
        if (node is null)
        {
            return null;
        }

        if (value.IsList || node is JsonArray)
        {
            if (node is not JsonArray array)
            {
                return DataValues.Coerce(node.ToString(), value.Type);
            }

            var items = array
                .Where(item => item is not null)
                .Select(item => item is JsonValue jsonValue ? DataValues.ToText(jsonValue) : item.ToJsonString())
                .Where(text => !string.IsNullOrEmpty(text))
                .ToArray();

            return items.Length == 0 ? null : string.Join(',', items);
        }

        return node is JsonValue scalar
            ? DataValues.Coerce(scalar, value.Type)
            : DataValues.Coerce(node.ToJsonString(), value.Type);
    }

    private sealed record ContentColumn(DataField Field, Func<ContentItem, object> Read);
}
