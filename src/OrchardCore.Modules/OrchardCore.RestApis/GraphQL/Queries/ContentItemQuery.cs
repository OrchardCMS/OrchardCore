using System.Threading.Tasks;
using GraphQL.Resolvers;
using GraphQL.Types;
using OrchardCore.ContentManagement;
using OrchardCore.RestApis.Types;

namespace OrchardCore.RestApis.GraphQL.Queries
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

            Resolver = new FuncFieldResolver<object, Task<object>>(async (context) => {
                return await contentManager.GetAsync(context.GetArgument<string>("contentItemId"));
            });
        }
    }
}
