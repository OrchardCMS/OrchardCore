using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class ContentItemQuery : QueryFieldType
    {
        public ContentItemQuery(IHttpContextAccessor httpContextAccessor)
        {
            Name = "ContentItem";

            Type = typeof(ContentItemType);

            Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {
                        Name = "contentItemId", Description = "content item id" }
                );

            Resolver = new AsyncFieldResolver<object, ContentItem>(async (context) =>
            {
                var contentManager = httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentManager>();
                var contentItem = await contentManager.GetAsync(context.GetArgument<string>("contentItemId"));

                //if (!await authorizationService.AuthorizeAsync((context.UserContext as GraphQLUserContext)?.User, Permissions.ViewContent, contentItem))
                //{
                //    return null;
                //}

                return contentItem;
            });
        }
    }
}
