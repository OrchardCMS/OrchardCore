using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.Apis.GraphQL.Types;
using OrchardCore.Contents.Apis.GraphQL.Queries.Types;

namespace OrchardCore.Contents.Apis.GraphQL.Queries
{
    public class ContentItemQuery : QueryFieldType
    {
        public ContentItemQuery(IContentManager contentManager) {
            Name = "ContentItem";

            Type = typeof(ContentItemType);

            Arguments = new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> {
                        Name = "contentItemId", Description = "content item id" }
                );

            Resolver = new SlowFuncFieldResolver<object, Task<ContentItem>>((context) => {
                return contentManager.GetAsync(context.GetArgument<string>("contentItemId"));
            });
        }
    }
}
