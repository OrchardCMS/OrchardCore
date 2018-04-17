using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class ContentItemsQuery : QueryFieldType
    {
        public ContentItemsQuery(IContentManager contentManager,
            IEnumerable<IGraphQLFilter<ContentItem>> graphQLFilters,
            ISession session)
        {
            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            Arguments = new QueryArguments(
                new QueryArgument<BooleanGraphType> { Name = "published", Description = "is the content item published", DefaultValue = true },
                new QueryArgument<BooleanGraphType> { Name = "latest", Description = "is the content item the latest version", DefaultValue = false },
                new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "content item id" },
                new QueryArgument<StringGraphType> { Name = "contentItemVersionId", Description = "the id of the version" }
            );

            Resolver = new AsyncFieldResolver<IEnumerable<ContentItem>>(async context => {
                var versionOptions = GetVersionOptions(context);

                if (context.HasPopulatedArgument("contentItemId"))
                {
                    var contentItem = await contentManager.GetAsync(context.GetArgument<string>("contentItemId"), versionOptions);

                    if (contentItem == null)
                    {
                        return Enumerable.Empty<ContentItem>();
                    }

                    return new[] { contentItem };
                }

                if (context.HasPopulatedArgument("contentItemVersionId"))
                {
                    var contentItem = await contentManager.GetVersionAsync(context.GetArgument<string>("contentItemVersionId"));

                    if (contentItem == null)
                    {
                        return Enumerable.Empty<ContentItem>();
                    }

                    return new[] { contentItem };
                }
                
                var query = session.Query<ContentItem, ContentItemIndex>();

                if (versionOptions.IsPublished)
                {
                    query = query.Where(q => q.Published == true);
                }

                if (versionOptions.IsLatest)
                {
                    query = query.Where(q => q.Latest == true);
                }

                if (context.HasPopulatedArgument("contentType"))
                {
                    var value = context.GetArgument<string>("contentType");
                    query = query.Where(q => q.ContentType == value);
                }
                else
                {
                    var value = (context.ReturnType as ListGraphType).ResolvedType.Name;
                    query = query.Where(q => q.ContentType == value);
                }

                IQuery<ContentItem> contentItemsQuery = query;

                foreach (var filter in graphQLFilters)
                {
                    contentItemsQuery = filter.PreQuery(contentItemsQuery, context);
                }

                var contentItems = await contentItemsQuery.ListAsync();

                foreach (var filter in graphQLFilters)
                {
                    contentItems = filter.PostQuery(contentItems, context);
                }
                
                return contentItems.ToList();
            });
        }

        private VersionOptions GetVersionOptions(ResolveFieldContext context)
        {
            if (context.HasPopulatedArgument("latest"))
            {
                var value = context.GetArgument<bool>("latest");
                if (value)
                {
                    return VersionOptions.Latest;
                }
            }

            if (context.HasPopulatedArgument("published"))
            {
                var value = context.GetArgument<bool>("published");
                if (value)
                {
                    return VersionOptions.Published;
                }
            }

            return VersionOptions.Published;
        }
    }
}
