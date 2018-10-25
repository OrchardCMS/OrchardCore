using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContentItemsFieldType(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            Name = "ContentItems";

            Type = typeof(ListGraphType<ContentItemType>);

            Arguments = new QueryArguments(
                new QueryArgument<BooleanGraphType> { Name = "published", Description = "whether to only include published versions", DefaultValue = true },
                new QueryArgument<StringGraphType> { Name = "contentType", Description = "type of content item" },
                new QueryArgument<StringGraphType> { Name = "contentItemId", Description = "content item id" },
                new QueryArgument<StringGraphType> { Name = "contentItemVersionId", Description = "the id of the version" }
            );

            Resolver = new AsyncFieldResolver<IEnumerable<ContentItem>>(Resolve);
        }

        private async Task<IEnumerable<ContentItem>> Resolve(ResolveFieldContext context)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var contentManager = httpContext.RequestServices.GetService<IContentManager>();

            var published = true;

            if (context.HasPopulatedArgument("published"))
            {
                published = context.GetArgument<bool>("published");
            }

            if (context.HasPopulatedArgument("contentItemId"))
            {
                var contentItemId = context.GetArgument<string>("contentItemId");

                var versionOption = published ? VersionOptions.Published : VersionOptions.Draft;

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

            var session = httpContext.RequestServices.GetService<YesSql.ISession>();
            var queryFilters = httpContext.RequestServices.GetServices<IGraphQLFilter<ContentItem>>();

            var query = session.Query<ContentItem, ContentItemIndex>();

            if (published)
            {
                query = query.Where(q => q.Published == true);
            }
            else
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
