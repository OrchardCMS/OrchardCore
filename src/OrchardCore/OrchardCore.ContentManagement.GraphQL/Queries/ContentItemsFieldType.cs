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
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// This type is used by <see cref="ContentTypeQuery"/> to represent a query on a content type
    /// </summary>
    public class ContentItemsFieldType : FieldType
    {
        public ContentItemsFieldType()
        {
            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            Arguments = new QueryArguments(
                new QueryArgument<ContentItemWhereInput> { Name = "where", Description = "filters the content items" },
                new QueryArgument<ContentItemOrderByInput> { Name = "orderBy", Description = "sort order" },
                new QueryArgument<IntGraphType> { Name = "first", Description = "the first n content items" },
                new QueryArgument<IntGraphType> { Name = "skip", Description = "the number of elements to skip" }
            );

            Resolver = new AsyncFieldResolver<IEnumerable<ContentItem>>(Resolve);
        }

        private async Task<IEnumerable<ContentItem>> Resolve(ResolveFieldContext context)
        {
            var graphContext = (GraphQLContext)context.UserContext;

            var whereInput = context.ArgumentAsObject<ContentItemWhereInputModel>("where");
            var versionOption = GetVersionOption(whereInput);

            if (whereInput != null)
            {
                if (!string.IsNullOrEmpty(whereInput.ContentItemId))
                {
                    return await GetContentItemById(whereInput.ContentItemId, versionOption, graphContext);
                }

                if (!string.IsNullOrEmpty(whereInput.ContentItemVersionId))
                {
                    return await GetContentItemByVersion(whereInput.ContentItemVersionId, graphContext);
                }
            }

            var session = graphContext.ServiceProvider.GetService<ISession>();
            var query = session.Query<ContentItem, ContentItemIndex>();

            query = Filter(query, context, whereInput, versionOption);
            query = OrderBy(query, context);

            IQuery<ContentItem> contentItemsQuery = query;
            var queryFilters = graphContext.ServiceProvider.GetServices<IGraphQLFilter<ContentItem>>().ToList();

            foreach (var filter in queryFilters)
            {
                contentItemsQuery = filter.PreQuery(query, context);
            }

            contentItemsQuery = PageQuery(contentItemsQuery, context);

            var contentItems = await contentItemsQuery.ListAsync();

            foreach (var filter in queryFilters)
            {
                contentItems = filter.PostQuery(contentItems, context);
            }

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

        private VersionOptions GetVersionOption(ContentItemWhereInputModel input)
        {
            if (input == null) return VersionOptions.Published;

            switch (input.Status)
            {
                case PublicationStatusEnum.Published: return VersionOptions.Published;
                case PublicationStatusEnum.Draft: return VersionOptions.Draft;
                case PublicationStatusEnum.Latest: return VersionOptions.Latest;
                case PublicationStatusEnum.All: return VersionOptions.AllVersions;
                default: return VersionOptions.Published;
            }
        }

        private async Task<IEnumerable<ContentItem>> GetContentItemById(string contentItemId,
            VersionOptions versionOption, GraphQLContext context)
        {
            var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
            var contentItem = await contentManager.GetAsync(contentItemId, versionOption);

            return contentItem == null ? Enumerable.Empty<ContentItem>() : new[] { contentItem };
        }

        private async Task<IEnumerable<ContentItem>> GetContentItemByVersion(string contentItemVersionId,
            GraphQLContext context)
        {
            var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
            var contentItem = await contentManager.GetVersionAsync(contentItemVersionId);

            return contentItem == null ? Enumerable.Empty<ContentItem>() : new[] { contentItem };
        }

        private IQuery<ContentItem, ContentItemIndex> Filter(
            IQuery<ContentItem, ContentItemIndex> query,
            ResolveFieldContext context,
            ContentItemWhereInputModel input,
            VersionOptions versionOption)
        {
            // Applying version

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

            // Applying content type

            var contentType = ((ListGraphType)context.ReturnType).ResolvedType.Name;

            query = query.Where(q => q.ContentType == contentType);

            if (input == null)
            {
                return query;
            }

            if (!string.IsNullOrEmpty(input.DisplayText))
            {
                query = query.Where(q => q.DisplayText == input.DisplayText);
            }

            if (input.ModifiedUtc.HasValue)
            {
                query = query.Where(q => q.ModifiedUtc == input.ModifiedUtc);
            }

            if (input.PublishedUtc.HasValue)
            {
                query = query.Where(q => q.PublishedUtc == input.PublishedUtc);
            }

            if (input.CreatedUtc.HasValue)
            {
                query = query.Where(q => q.CreatedUtc == input.CreatedUtc);
            }

            if (!string.IsNullOrEmpty(input.Owner))
            {
                query = query.Where(q => q.Owner == input.Owner);
            }

            if (!string.IsNullOrEmpty(input.Author))
            {
                query = query.Where(q => q.Author == input.Author);
            }

            return query;
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
                            case "contentType": selector = x => x.ContentType; break;
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