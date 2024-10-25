using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Resolvers;
using OrchardCore.ContentManagement.GraphQL.Options;
using OrchardCore.ContentManagement.GraphQL.Queries.Predicates;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;
using Expression = OrchardCore.ContentManagement.GraphQL.Queries.Predicates.Expression;

namespace OrchardCore.ContentManagement.GraphQL.Queries;

/// <summary>
/// This type is used by <see cref="ContentTypeQuery"/> to represent a query on a content type.
/// </summary>
public class ContentItemsFieldType : FieldType
{
    private static readonly List<string> _contentItemProperties;
    private readonly int _defaultNumberOfItems;

    static ContentItemsFieldType()
    {
        _contentItemProperties = [];

        foreach (var property in typeof(ContentItemIndex).GetProperties())
        {
            _contentItemProperties.Add(property.Name);
        }
    }

    public ContentItemsFieldType(string contentItemName, ISchema schema, IOptions<GraphQLContentOptions> optionsAccessor, IOptions<GraphQLSettings> settingsAccessor)
    {
        Name = "ContentItems";

        Type = typeof(ListGraphType<ContentItemType>);

        var whereInput = new ContentItemWhereInput(contentItemName, optionsAccessor);
        var orderByInput = new ContentItemOrderByInput(contentItemName);

        Arguments = new QueryArguments(
            new QueryArgument<ContentItemWhereInput> { Name = "where", Description = "filters the content items", ResolvedType = whereInput },
            new QueryArgument<ContentItemOrderByInput> { Name = "orderBy", Description = "sort order", ResolvedType = orderByInput },
            new QueryArgument<IntGraphType> { Name = "first", Description = "the first n content items", ResolvedType = new IntGraphType() },
            new QueryArgument<IntGraphType> { Name = "skip", Description = "the number of content items to skip", ResolvedType = new IntGraphType() },
            new QueryArgument<PublicationStatusGraphType> { Name = "status", Description = "publication status of the content item", ResolvedType = new PublicationStatusGraphType(), DefaultValue = PublicationStatusEnum.Published }
        );

        Resolver = new LockedAsyncFieldResolver<IEnumerable<ContentItem>>(ResolveAsync);

        schema.RegisterType(whereInput);
        schema.RegisterType(orderByInput);
        schema.RegisterType<PublicationStatusGraphType>();

        _defaultNumberOfItems = settingsAccessor.Value.DefaultNumberOfResults;
    }

    private async ValueTask<IEnumerable<ContentItem>> ResolveAsync(IResolveFieldContext context)

    {
        JsonObject where = null;
        if (context.HasArgument("where"))
        {
            // 'context.Arguments[].Value' is never null in GraphQL.NET 4.
            where = JObject.FromObject(context.Arguments["where"].Value);
        }

        var session = context.RequestServices.GetService<ISession>();

        var preQuery = session.Query<ContentItem>();

        var filters = context.RequestServices.GetServices<IGraphQLFilter<ContentItem>>();

        foreach (var filter in filters)
        {
            preQuery = await filter.PreQueryAsync(preQuery, context);
        }

        var query = preQuery.With<ContentItemIndex>();

        query = FilterVersion(query, GetVersionOptions(context));
        query = FilterContentType(query, context);
        query = OrderBy(query, context);

        var contentItemsQuery = await FilterWhereArgumentsAsync(query, where, context, session);
        contentItemsQuery = PageQuery(contentItemsQuery, context);

        var contentItems = await contentItemsQuery.ListAsync();

        foreach (var filter in filters)
        {
            contentItems = await filter.PostQueryAsync(contentItems, context);
        }

        return contentItems;
    }

    private async ValueTask<IQuery<ContentItem>> FilterWhereArgumentsAsync(
        IQuery<ContentItem, ContentItemIndex> query,
        JsonObject where,
        IResolveFieldContext fieldContext,
        ISession session)
    {
        if (where == null)
        {
            return query;
        }

        var defaultTableAlias = query.GetTypeAlias(typeof(ContentItemIndex));

        var predicateQuery = new PredicateQuery(
            configuration: session.Store.Configuration,
            propertyProviders: fieldContext.RequestServices.GetServices<IIndexPropertyProvider>());

        // Create the default table alias.
        predicateQuery.CreateAlias(string.Empty, nameof(ContentItemIndex));
        predicateQuery.CreateTableAlias(nameof(ContentItemIndex), defaultTableAlias);

        // Add all provided table alias to the current predicate query.
        var providers = fieldContext.RequestServices.GetServices<IIndexAliasProvider>();
        var indexes = new Dictionary<string, IndexAlias>(StringComparer.OrdinalIgnoreCase);
        var indexAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var aliasProvider in providers)
        {
            foreach (var alias in await aliasProvider.GetAliasesAsync())
            {
                predicateQuery.CreateAlias(alias.Alias, alias.Index);
                if (indexAliases.TryAdd(alias.Alias, alias.Alias))
                {
                    indexes.TryAdd(alias.Index, alias);
                }
            }
        }

        var expressions = Expression.Conjunction();
        BuildWhereExpressions(where, expressions, null, fieldContext, indexAliases);
        expressions.SearchUsedAlias(predicateQuery);

        // Add all Indexes that were used in the predicate query.
        IQuery<ContentItem> contentQuery = query;
        foreach (var usedAlias in predicateQuery.GetUsedAliases())
        {
            if (indexes.TryGetValue(usedAlias, out var indexAlias))
            {
                contentQuery = contentQuery.With(indexAlias.IndexType);
                var tableAlias = query.GetTypeAlias(indexAlias.IndexType);
                predicateQuery.CreateTableAlias(indexAlias.Index, tableAlias);
            }
        }

        var whereSqlClause = expressions.ToSqlString(predicateQuery);


        query = query.Where(whereSqlClause);

        // Add all parameters that were used in the predicate query.
        foreach (var parameter in predicateQuery.Parameters)
        {
            query = query.WithParameter(parameter.Key, parameter.Value);
        }

        return contentQuery;
    }

    private IQuery<ContentItem> PageQuery(IQuery<ContentItem> contentItemsQuery, IResolveFieldContext context)
    {
        var first = context.GetArgument<int>("first");

        if (first == 0)
        {
            first = _defaultNumberOfItems;
        }

        contentItemsQuery = contentItemsQuery.Take(first);

        if (context.HasPopulatedArgument("skip"))
        {
            var skip = context.GetArgument<int>("skip");

            contentItemsQuery = contentItemsQuery.Skip(skip);
        }

        return contentItemsQuery;
    }

    private static VersionOptions GetVersionOption(PublicationStatusEnum status)
    {
        return status switch
        {
            PublicationStatusEnum.Published => VersionOptions.Published,
            PublicationStatusEnum.Draft => VersionOptions.Draft,
            PublicationStatusEnum.Latest => VersionOptions.Latest,
            _ => VersionOptions.Published,
        };
    }

    private static IQuery<ContentItem, ContentItemIndex> FilterContentType(IQuery<ContentItem, ContentItemIndex> query, IResolveFieldContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var contentType = ((ListGraphType)(context.FieldDefinition).ResolvedType).ResolvedType.Name;
        return query.Where(q => q.ContentType == contentType);
    }

    private static VersionOptions GetVersionOptions(IResolveFieldContext context)
    {
        if (context.HasPopulatedArgument("status"))
        {
            return GetVersionOption(context.GetArgument<PublicationStatusEnum>("status"));
        }

        return VersionOptions.Published;
    }

    private static IQuery<ContentItem, ContentItemIndex> FilterVersion(IQuery<ContentItem, ContentItemIndex> query, VersionOptions versionOption)
    {
        if (versionOption.IsPublished)
        {
            query = query.Where(q => q.Published == true);
        }
        else if (versionOption.IsDraft)
        {
            query = query.Where(q => q.Latest == true && q.Published == false);
        }
        else if (versionOption.IsLatest)
        {
            query = query.Where(q => q.Latest == true);
        }

        return query;
    }

    private void BuildWhereExpressions(JsonNode where, Junction expressions, string tableAlias, IResolveFieldContext fieldContext, IDictionary<string, string> indexAliases)
    {
        if (where is JsonArray array)
        {
            foreach (var child in array)
            {
                if (child is JsonObject whereObject)
                {
                    BuildExpressionsInternal(whereObject, expressions, tableAlias, fieldContext, indexAliases);
                }
            }
        }
        else if (where is JsonObject whereObject)
        {
            BuildExpressionsInternal(whereObject, expressions, tableAlias, fieldContext, indexAliases);
        }
    }

    private void BuildExpressionsInternal(JsonObject where, Junction expressions, string tableAlias, IResolveFieldContext fieldContext, IDictionary<string, string> indexAliases)
    {
        foreach (var entry in where)
        {
            // New typed arguments return default null values.
            if (entry.Value.GetValueKind() == JsonValueKind.Undefined || entry.Value.GetValueKind() == JsonValueKind.Null)
            {
                continue;
            }

            IPredicate expression = null;

            var values = entry.Key.Split('_', 2);

            // Gets the full path name without the comparison e.g. aliasPart.alias, not aliasPart.alias_contains.
            var property = values[0];

            // Figure out table aliases for collapsed parts and ones with the part suffix removed by the dsl.
            if (tableAlias == null || !tableAlias.EndsWith("Part", StringComparison.OrdinalIgnoreCase))
            {
                var whereArgument = fieldContext?.FieldDefinition.Arguments.FirstOrDefault(x => x.Name == "where");

                if (whereArgument != null)
                {
                    var whereInput = (WhereInputObjectGraphType)whereArgument.ResolvedType;

                    foreach (var field in whereInput.Fields.Where(x => x.GetMetadata<string>("PartName") != null))
                    {
                        var partName = field.GetMetadata<string>("PartName");
                        if ((tableAlias == null && field.GetMetadata<bool>("PartCollapsed") && field.Name.Equals(property, StringComparison.OrdinalIgnoreCase)) ||
                            (tableAlias != null && partName.ToFieldName().Equals(tableAlias, StringComparison.OrdinalIgnoreCase)))
                        {
                            tableAlias = indexAliases.TryGetValue(partName, out var indexTableAlias) ? indexTableAlias : tableAlias;
                            break;
                        }
                    }
                }
            }

            if (tableAlias != null)
            {
                property = $"{tableAlias}.{property}";
            }

            if (values.Length == 1)
            {
                if (string.Equals(values[0], "or", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Disjunction();
                    BuildWhereExpressions(entry.Value, (Junction)expression, tableAlias, fieldContext, indexAliases);
                }
                else if (string.Equals(values[0], "and", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Conjunction();
                    BuildWhereExpressions(entry.Value, (Junction)expression, tableAlias, fieldContext, indexAliases);
                }
                else if (string.Equals(values[0], "not", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Conjunction();
                    BuildWhereExpressions(entry.Value, (Junction)expression, tableAlias, fieldContext, indexAliases);
                    expression = Expression.Not(expression);
                }
                else if (entry.Value.HasValues() && entry.Value.GetValueKind() == JsonValueKind.Object)
                {
                    // Loop through the part's properties, passing the name of the part as the table tableAlias.
                    // This tableAlias can then be used with the table alias to index mappings to join with the correct table.
                    BuildWhereExpressions(entry.Value, expressions, values[0], fieldContext, indexAliases);
                }
                else
                {
                    var propertyValue = entry.Value.ToObject<object>();
                    expression = Expression.Equal(property, propertyValue);
                }
            }
            else
            {
                var value = entry.Value.ToObject<object>();

                expression = values[1] switch
                {
                    "not" => Expression.Not(Expression.Equal(property, value)),
                    "gt" => Expression.GreaterThan(property, value),
                    "gte" => Expression.GreaterThanOrEqual(property, value),
                    "lt" => Expression.LessThan(property, value),
                    "lte" => Expression.LessThanOrEqual(property, value),
                    "contains" => Expression.Like(property, (string)value, MatchOptions.Contains),
                    "not_contains" => Expression.Not(Expression.Like(property, (string)value, MatchOptions.Contains)),
                    "starts_with" => Expression.Like(property, (string)value, MatchOptions.StartsWith),
                    "not_starts_with" => Expression.Not(Expression.Like(property, (string)value, MatchOptions.StartsWith)),
                    "ends_with" => Expression.Like(property, (string)value, MatchOptions.EndsWith),
                    "not_ends_with" => Expression.Not(Expression.Like(property, (string)value, MatchOptions.EndsWith)),
                    "in" => Expression.In(property, entry.Value.ToObject<object[]>()),
                    "not_in" => Expression.Not(Expression.In(property, entry.Value.ToObject<object[]>())),
                    _ => Expression.Equal(property, value),
                };
            }

            if (expression != null)
            {
                expressions.Add(expression);
            }
        }
    }

    private static IQuery<ContentItem, ContentItemIndex> OrderBy(IQuery<ContentItem, ContentItemIndex> query,
        IResolveFieldContext context)
    {
        if (context.HasPopulatedArgument("orderBy"))
        {
            var orderByArguments = JObject.FromObject(context.Arguments["orderBy"].Value);

            if (orderByArguments != null)
            {
                var thenBy = false;

                foreach (var property in orderByArguments)
                {
                    var direction = (OrderByDirection)property.Value.Value<int>();

                    Expression<Func<ContentItemIndex, object>> selector = null;

                    switch (property.Key)
                    {
                        case "contentItemId": selector = x => x.ContentItemId; break;
                        case "contentItemVersionId": selector = x => x.ContentItemVersionId; break;
                        case "displayText": selector = x => x.DisplayText; break;
                        case "published": selector = x => x.Published; break;
                        case "latest": selector = x => x.Latest; break;
                        case "createdUtc": selector = x => x.CreatedUtc; break;
                        case "modifiedUtc": selector = x => x.ModifiedUtc; break;
                        case "publishedUtc": selector = x => x.PublishedUtc; break;
                        case "owner": selector = x => x.Owner; break;
                        case "author": selector = x => x.Author; break;
                    }

                    if (selector != null)
                    {
                        if (!thenBy)
                        {
                            query = direction == OrderByDirection.Ascending
                                    ? query.OrderBy(selector)
                                    : query.OrderByDescending(selector)
                                ;
                        }
                        else
                        {
                            query = direction == OrderByDirection.Ascending
                                    ? query.ThenBy(selector)
                                    : query.ThenByDescending(selector)
                                ;
                        }

                        thenBy = true;
                    }
                }
            }
        }
        else
        {
            query = query.OrderByDescending(x => x.CreatedUtc);
        }

        return query;
    }
}
