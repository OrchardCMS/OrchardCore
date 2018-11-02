using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.Apis.GraphQL;
using OrchardCore.ContentManagement.GraphQL.Queries.Predicates;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;
using Expression = OrchardCore.ContentManagement.GraphQL.Queries.Predicates.Expression;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// This type is used by <see cref="ContentTypeQuery"/> to represent a query on a content type
    /// </summary>
    public class ContentItemsFieldType : FieldType
    {

        private static readonly List<string> ContentItemProperties;

        static ContentItemsFieldType()
        {
            ContentItemProperties = new List<string>();

            foreach (var property in typeof(ContentItemIndex).GetProperties())
            {
                ContentItemProperties.Add(property.Name);
            }
        }

        public ContentItemsFieldType(string contentItemName, ISchema schema)
        {
            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            var whereInput = new ContentItemWhereInput(contentItemName);
            var orderByInput = new ContentItemOrderByInput(contentItemName);

            Arguments = new QueryArguments(
                new QueryArgument<ContentItemWhereInput> { Name = "where", Description = "filters the content items", ResolvedType = whereInput },
                new QueryArgument<ContentItemOrderByInput> { Name = "orderBy", Description = "sort order", ResolvedType = orderByInput },
                new QueryArgument<IntGraphType> { Name = "first", Description = "the first n content items", ResolvedType = new IntGraphType() },
                new QueryArgument<IntGraphType> { Name = "skip", Description = "the number of elements to skip", ResolvedType = new IntGraphType() },
                new QueryArgument<PublicationStatusGraphType> { Name = "status", Description = "publication status of the content item", ResolvedType = new PublicationStatusGraphType(), DefaultValue = PublicationStatusEnum.Published }
            );

            Resolver = new AsyncFieldResolver<IEnumerable<ContentItem>>(Resolve);

            schema.RegisterType(whereInput);
            schema.RegisterType(orderByInput);
            schema.RegisterType<PublicationStatusGraphType>();
        }

        private async Task<IEnumerable<ContentItem>> Resolve(ResolveFieldContext context)
        {
            var graphContext = (GraphQLContext)context.UserContext;

            var versionOption = VersionOptions.Published;

            if (context.HasPopulatedArgument("status"))
            {
                versionOption = GetVersionOption(context.GetArgument<PublicationStatusEnum>("status"));
            }

            JObject where = null;
            if (context.HasArgument("where"))
            {
                where = JObject.FromObject(context.Arguments["where"]);
            }

            var session = graphContext.ServiceProvider.GetService<ISession>();

            var query = session.Query<ContentItem, ContentItemIndex>();

            query = FilterVersion(query, versionOption);
            query = FilterContentType(query, context);
            query = OrderBy(query, context);

            var expressions = Expression.Conjunction();
            BuildWhereExpressions(where, expressions);

            var predicateQuery = graphContext.ServiceProvider.GetService<IPredicateQuery>();
            query = query.Where(expressions.ToSqlString(predicateQuery));

            IQuery<ContentItem> contentItemsQuery = query;
            contentItemsQuery = PageQuery(contentItemsQuery, context);

            var contentItems = await contentItemsQuery.ListAsync();

            return contentItems.ToList();
        }

        private IQuery<ContentItem> PageQuery(IQuery<ContentItem> contentItemsQuery, ResolveFieldContext context)
        {
            if (context.HasPopulatedArgument("first"))
            {
                var first = context.GetArgument<int>("first");

                contentItemsQuery = contentItemsQuery.Take(first);
            }

            if (context.HasPopulatedArgument("skip"))
            {
                var skip = context.GetArgument<int>("skip");

                contentItemsQuery = contentItemsQuery.Skip(skip);
            }

            return contentItemsQuery;
        }

        private VersionOptions GetVersionOption(PublicationStatusEnum status)
        {
            switch (status)
            {
                case PublicationStatusEnum.Published: return VersionOptions.Published;
                case PublicationStatusEnum.Draft: return VersionOptions.Draft;
                case PublicationStatusEnum.Latest: return VersionOptions.Latest;
                case PublicationStatusEnum.All: return VersionOptions.AllVersions;
                default: return VersionOptions.Published;
            }
        }

        private static IQuery<ContentItem, ContentItemIndex> FilterContentType(IQuery<ContentItem, ContentItemIndex> query, ResolveFieldContext context)
        {
            var contentType = ((ListGraphType) context.ReturnType).ResolvedType.Name;

            return query.Where(q => q.ContentType == contentType);
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

        private void BuildWhereExpressions(JToken where, Junction expressions)
        {
            if (where is JArray array)
            {
                foreach (var child in array.Children())
                {
                    if (child is JObject whereObject)
                    {
                        BuildExpressionsInternal(whereObject, expressions);
                    }
                }
            }
            else if(where is JObject whereObject)
            {
                BuildExpressionsInternal(whereObject, expressions);
            }
        }

        private void BuildExpressionsInternal(JObject where, Junction expressions)
        {
            foreach (var entry in where.Properties())
            {
                var values = entry.Name.Split(new[] {'_'}, 2);

                var property = values[0];

                if (property == "status")
                {
                    continue;
                }

                if (values.Length == 1)
                {
                    if (String.Equals(property, "or", StringComparison.OrdinalIgnoreCase))
                    {
                        var comparison = Expression.Disjunction();
                        BuildWhereExpressions(entry.Value, comparison);
                        expressions.Add(comparison);
                    }
                    else if (String.Equals(property, "and", StringComparison.OrdinalIgnoreCase))
                    {
                        var comparison = Expression.Conjunction();
                        BuildWhereExpressions(entry.Value, comparison);
                        expressions.Add(comparison);
                    }
                    else if (String.Equals(property, "not", StringComparison.OrdinalIgnoreCase))
                    {
                        var comparison = Expression.Conjunction();
                        BuildWhereExpressions(entry.Value, comparison);
                        expressions.Add(Expression.Not(comparison));
                    }
                    else
                    {     
                        var propertyValue = entry.Value.ToObject<object>();
                        var comparison = Expression.Equal(property, propertyValue);
                        expressions.Add(comparison);
                    }
                }
                else
                {
                    IPredicate comparison;

                    var value = entry.Value.ToObject<object>();

                    switch (values[1])
                    {
                        case "not": comparison = Expression.Not(Expression.Equal(property, value)); break;
                        case "gt": comparison = Expression.GreaterThan(property, value); break;
                        case "gte": comparison = Expression.GreaterThanOrEqual(property, value); break;
                        case "lt": comparison = Expression.LessThan(property, value); break;
                        case "lte": comparison = Expression.LessThanOrEqual(property, value); break;
                        case "contains": comparison = Expression.Like(property, (string)value, MatchOptions.Contains); break;
                        case "not_contains": comparison = Expression.Not(Expression.Like(property, (string)value, MatchOptions.Contains)); break;
                        case "starts_with": comparison = Expression.Like(property, (string)value, MatchOptions.StartsWith); break;
                        case "not_starts_with": comparison = Expression.Not(Expression.Like(property, (string)value, MatchOptions.StartsWith)); break;
                        case "ends_with": comparison = Expression.Like(property, (string)value, MatchOptions.EndsWith); break;
                        case "not_ends_with": comparison = Expression.Not(Expression.Like(property, (string)value, MatchOptions.EndsWith)); break;
                        case "in": comparison = Expression.In(property, entry.Value.ToObject<object[]>()); break;
                        case "not_in": comparison = Expression.In(property, entry.Value.ToObject<object[]>()); break;

                        default: comparison = Expression.Equal(property, value); break;
                    }

                    expressions.Add(comparison);
                }    
            }
        }

        private IQuery<ContentItem, ContentItemIndex> OrderBy(IQuery<ContentItem, ContentItemIndex> query,
            ResolveFieldContext context)
        {
            if (context.HasPopulatedArgument("orderBy"))
            {
                var orderByArguments = JObject.FromObject(context.Arguments["orderBy"]);

                if (orderByArguments != null)
                {
                    var thenBy = false;

                    foreach (var property in orderByArguments.Properties())
                    {
                        var direction = (OrderByDirection)property.Value.Value<int>();

                        Expression<Func<ContentItemIndex, object>> selector = null;

                        switch (property.Name)
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
}