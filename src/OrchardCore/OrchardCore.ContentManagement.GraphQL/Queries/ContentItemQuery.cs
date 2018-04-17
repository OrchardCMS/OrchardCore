using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using OrchardCore.Apis.GraphQL;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class ContentItemQuery : QueryFieldType
    {
        public ContentItemQuery(IContentManager contentManager,
            IAuthorizationService authorizationService) {
            Name = "ContentItem";

            Type = typeof(ContentItemType);

            Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {
                        Name = "contentItemId", Description = "content item id" }
                );

            Resolver = new AsyncFieldResolver<object, ContentItem>(async (context) => {
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
