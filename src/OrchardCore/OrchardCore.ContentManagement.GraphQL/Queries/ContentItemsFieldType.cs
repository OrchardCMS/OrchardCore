using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Records;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    /// <summary>
    /// This type is used by <see cref="ContentItemFieldTypeProvider"/> to represent a query on a content type
    /// </summary>
    public class ContentItemsFieldType : FieldType
    {
        public ContentItemsFieldType()
        {
            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            Arguments = new QueryArguments(
                new QueryArgument<PublicationStatusGraphType> { Name = "status", Description = "publication status of the content item", DefaultValue = PublicationStatusEnum.Published },
                new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "content item id" },
                new QueryArgument<StringGraphType> { Name = "contentItemVersionId", Description = "the id of the version" }
            );

            Resolver = new AsyncFieldResolver<IEnumerable<ContentItem>>(Resolve);
        }

        private async Task<IEnumerable<ContentItem>> Resolve(ResolveFieldContext context)
        {
            var graphContext = (GraphQLContext)context.UserContext;

            var contentManager = graphContext.ServiceProvider.GetRequiredService<IContentManager>();

            var status = PublicationStatusEnum.Published;

            var versionOption = VersionOptions.Published;

            if (context.HasPopulatedArgument("status"))
            {
                status = context.GetArgument<PublicationStatusEnum>("status");
            }

            switch (status)
            {
                case PublicationStatusEnum.Published: versionOption = VersionOptions.Published; break;
                case PublicationStatusEnum.Draft: versionOption = VersionOptions.Draft; break;
                case PublicationStatusEnum.Latest: versionOption = VersionOptions.Latest; break;
            }

            if (context.HasPopulatedArgument("contentItemId"))
            {
                var contentItemId = context.GetArgument<string>("contentItemId");

                var contentItem = await contentManager.GetAsync(contentItemId, versionOption);

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

            var session = graphContext.ServiceProvider.GetService<YesSql.ISession>();
            var queryFilters = graphContext.ServiceProvider.GetServices<IGraphQLFilter<ContentItem>>();

            var query = session.Query<ContentItem, ContentItemIndex>();

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

            foreach (var filter in queryFilters)
            {
                contentItemsQuery = filter.PreQuery(contentItemsQuery, context);
            }

            var contentItems = await contentItemsQuery.ListAsync();

            foreach (var filter in queryFilters)
            {
                contentItems = filter.PostQuery(contentItems, context);
            }

            return contentItems.ToList();
        }
    }
}
