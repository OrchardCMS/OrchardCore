using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
    private readonly int _defaultNumberOfItems;

    public ContentItemsFieldType(
        string contentItemName,
        ISchema schema,
        IOptions<GraphQLContentOptions> optionsAccessor,
        IOptions<GraphQLSettings> settingsAccessor,
        IServiceProvider serviceProvider)
    {
        Name = contentItemName;

        Type = typeof(ListGraphType<ContentItemType>);

        var S = serviceProvider.GetRequiredService<IStringLocalizer<ContentItemsFieldType>>();

        var whereInput = new ContentItemWhereInput(contentItemName, optionsAccessor, serviceProvider.GetRequiredService<IStringLocalizer<ContentItemWhereInput>>());
        var orderByInput = new ContentItemOrderByInput(contentItemName);

        Arguments = new QueryArguments(
            new QueryArgument<ContentItemWhereInput>
            {
                Name = "where",
                Description = S["filters the content items"],
                ResolvedType = whereInput,
            },
            new QueryArgument<ContentItemOrderByInput>
            {
                Name = "orderBy",
                Description = S["sort order"],
                ResolvedType = orderByInput,
            },
            new QueryArgument<IntGraphType>
            {
                Name = "first",
                Description = S["the first n content items"],
                ResolvedType = new IntGraphType(),
            },
            new QueryArgument<IntGraphType>
            {
                Name = "skip",
                Description = S["the number of content items to skip"],
                ResolvedType = new IntGraphType(),
            },
            new QueryArgument<PublicationStatusGraphType>
            {
                Name = "status",
                Description = S["publication status of the content item"],
                ResolvedType = new PublicationStatusGraphType(serviceProvider.GetRequiredService<IStringLocalizer<PublicationStatusGraphType>>()),
                DefaultValue = PublicationStatusEnum.Published,
            }
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

        query = FilterVersion(query, context.GetArgument<PublicationStatusEnum>("status"));
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
        var indexAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var aliasProvider in providers)
        {
            foreach (var alias in await aliasProvider.GetAliasesAsync())
            {
                predicateQuery.CreateAlias(alias.Alias, alias.Index, alias.IsPartial);
                if (indexAliases.Add(alias.Alias))
                {
                    if (!indexes.TryAdd(alias.Index, alias))
                    {
                        if (indexes[alias.Index].IndexType != alias.IndexType)
                        {
                            throw new InvalidOperationException("An ambiguous index has been found.");
                        }
                    }
                }
            }
        }

        var expressions = Expression.Conjunction();

        var whereInput = (IFilterInputObjectGraphType)fieldContext.FieldDefinition.Arguments.FirstOrDefault(x => x.Name == "where")?.ResolvedType;

        BuildWhereExpressions(where, expressions, null, whereInput, indexAliases);

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

    private static IQuery<ContentItem, ContentItemIndex> FilterContentType(IQuery<ContentItem, ContentItemIndex> query, IResolveFieldContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var contentType = ((ListGraphType)(context.FieldDefinition).ResolvedType).ResolvedType.Name;
        return query.Where(q => q.ContentType == contentType);
    }

    private static IQuery<ContentItem, ContentItemIndex> FilterVersion(IQuery<ContentItem, ContentItemIndex> query, PublicationStatusEnum status)
    {
        if (status == PublicationStatusEnum.Published)
        {
            query = query.Where(q => q.Published);
        }
        else if (status == PublicationStatusEnum.Draft)
        {
            query = query.Where(q => q.Latest && !q.Published);
        }
        else if (status == PublicationStatusEnum.Latest)
        {
            query = query.Where(q => q.Latest);
        }

        return query;
    }

    private void BuildWhereExpressions(JsonNode where, Junction expressions, string tableAlias, IFilterInputObjectGraphType filterInputGraphType, HashSet<string> indexAliases)
    {
        if (where is JsonArray array)
        {
            foreach (var child in array)
            {
                if (child is JsonObject whereObject)
                {
                    BuildExpressionsInternal(whereObject, expressions, tableAlias, filterInputGraphType, indexAliases);
                }
            }
        }
        else if (where is JsonObject whereObject)
        {
            BuildExpressionsInternal(whereObject, expressions, tableAlias, filterInputGraphType, indexAliases);
        }
    }

    private void BuildExpressionsInternal(JsonObject where, Junction expressions, string tableAlias, IFilterInputObjectGraphType filterInputGraphType, HashSet<string> indexAliases)
    {
        foreach (var entry in where)
        {
            // New typed arguments return default null values.
            if (entry.Value.GetValueKind() == JsonValueKind.Undefined || entry.Value.GetValueKind() == JsonValueKind.Null)
            {
                continue;
            }

            IPredicate expression = null;

            // Gets the full path name without the comparison e.g. aliasPart.alias, not aliasPart.alias_contains.
            var values = entry.Key.Split('_', 2);
            var fieldName = values[0];

            // Get the actual field used, to get the alias name and additional required expressions for indexed content fields.
            var currentField = filterInputGraphType?.Fields.Where(field => field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            var aliasName = currentField?.GetMetadata<string>("AliasName");
            var property = aliasName;

            if (string.IsNullOrEmpty(property))
            {
                property = fieldName;

                // Figure out table aliases for collapsed parts and ones with the part suffix removed by the dsl.
                if (tableAlias == null || !tableAlias.EndsWith("Part", StringComparison.OrdinalIgnoreCase))
                {
                    if (filterInputGraphType != null)
                    {
                        foreach (var field in filterInputGraphType.Fields.Where(x => x.GetMetadata<string>("PartName") != null))
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
            }

            if (values.Length == 1)
            {
                if (string.Equals(values[0], "or", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Disjunction();
                    BuildWhereExpressions(entry.Value, (Junction)expression, tableAlias, currentField?.ResolvedType as IFilterInputObjectGraphType ?? filterInputGraphType, indexAliases);
                }
                else if (string.Equals(values[0], "and", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Conjunction();
                    BuildWhereExpressions(entry.Value, (Junction)expression, tableAlias, currentField?.ResolvedType as IFilterInputObjectGraphType ?? filterInputGraphType, indexAliases);
                }
                else if (string.Equals(values[0], "not", StringComparison.OrdinalIgnoreCase))
                {
                    expression = Expression.Conjunction();
                    BuildWhereExpressions(entry.Value, (Junction)expression, tableAlias, currentField?.ResolvedType as IFilterInputObjectGraphType ?? filterInputGraphType, indexAliases);
                    expression = Expression.Not(expression);
                }
                else if (entry.Value.HasValues() && entry.Value.GetValueKind() == JsonValueKind.Object)
                {
                    // Loop through the part's properties, passing the name of the part as the table tableAlias.
                    // This tableAlias can then be used with the table alias to index mappings to join with the correct table.
                    BuildWhereExpressions(entry.Value, expressions, values[0], currentField?.ResolvedType as IFilterInputObjectGraphType ?? filterInputGraphType, indexAliases);
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
                // For indexed content fields, add the additionally required columns.
                if (!string.IsNullOrEmpty(aliasName))
                {
                    var contentPart = currentField.GetMetadata<string>("ContentPart");
                    var contentField = currentField.GetMetadata<string>("ContentField");

                    if (!string.IsNullOrEmpty(contentPart) || !string.IsNullOrEmpty(contentField))
                    {
                        var andExpression = Expression.Conjunction();

                        if (!string.IsNullOrEmpty(contentPart))
                        {
                            andExpression.Add(Expression.Equal($"{aliasName}:ContentPart", contentPart));
                        }

                        if (!string.IsNullOrEmpty(contentField))
                        {
                            andExpression.Add(Expression.Equal($"{aliasName}:ContentField", contentField));
                        }

                        andExpression.Add(expression);

                        expression = andExpression;
                    }
                }

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
                    var direction = property.Value.GetEnumValue<OrderByDirection>();

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
